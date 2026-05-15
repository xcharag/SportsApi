using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using SportsApi.domain.Abstractions.Auth;
using SportsApi.domain.Abstractions.Messaging;
using SportsApi.domain.Abstractions.Persistence;
using SportsApi.infrastructure.Persistence.Core.EntityFramework;
using SportsApi.infrastructure.Persistence.Core.EntityFramework.Repositories;
using SportsApi.infrastructure.Services.Auth;
using SportsApi.infrastructure.Services.Live;
using SportsApi.infrastructure.Services.Messaging;
using IMatchLiveHub = SportsApi.infrastructure.Services.Live.IMatchLiveHub;

namespace SportsApi.infrastructure;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Try CoreConnection first, fallback to LocalConnection for development
        var connectionString = configuration.GetConnectionString("CoreConnection") 
                            ?? configuration.GetConnectionString("LocalConnection");
        
        services.AddDbContext<CoreDbContext>(options =>
            options.UseNpgsql(
                connectionString,
                npgsqlOptions => npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorCodesToAdd: null)));

        services.AddScoped<IMediator, Mediator>();
        services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
        services.AddScoped<ICoreUnitOfWork, EfUnitOfWork>();
        
        var jwtSettings = configuration.GetSection("JwtSettings");
        var authority = jwtSettings["Authority"];
        var secretKey = jwtSettings["SecretKey"];

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            if (!string.IsNullOrWhiteSpace(authority))
            {
                options.Authority = authority;
                options.Audience = jwtSettings["Audience"];
                options.RequireHttpsMetadata = authority.StartsWith("https://", StringComparison.OrdinalIgnoreCase);

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var token = context.Request.Cookies["accessToken"];

                        if (string.IsNullOrWhiteSpace(token))
                            token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(' ').Last();

                        // SSE clients cannot set headers; allow token in query string
                        if (string.IsNullOrWhiteSpace(token))
                            token = context.Request.Query["access_token"];

                        if (!string.IsNullOrWhiteSpace(token))
                            context.Token = token;

                        return Task.CompletedTask;
                    },
                    OnAuthenticationFailed = context =>
                    {
                        try
                        {
                            var loggerFactory = context.HttpContext.RequestServices.GetService<ILoggerFactory>();
                            var logger = loggerFactory?.CreateLogger("JwtAuth");
                            logger?.LogError(context.Exception, "JWT authentication failed: {Message}", context.Exception.Message);
                        }
                        catch
                        {
                            // ignored
                        }

                        return Task.CompletedTask;
                    },
                    OnChallenge = context =>
                    {
                        try
                        {
                            var loggerFactory = context.HttpContext.RequestServices.GetService<ILoggerFactory>();
                            var logger = loggerFactory?.CreateLogger("JwtAuth");
                            logger?.LogWarning("JWT challenge: {Error} - {ErrorDescription}", context.Error, context.ErrorDescription);
                        }
                        catch
                        {
                            // ignored
                        }

                        return Task.CompletedTask;
                    }
                };
            }
            else
            {
                if (string.IsNullOrWhiteSpace(secretKey))
                    throw new InvalidOperationException(
                        "JwtSettings:SecretKey is not configured. It MUST match the SISAPI auth microservice secret key, or set JwtSettings:Authority for JWKS.");

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidAudience = jwtSettings["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(secretKey)),
                    ClockSkew = TimeSpan.Zero
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var token = context.Request.Cookies["accessToken"];

                        if (string.IsNullOrWhiteSpace(token))
                            token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(' ').Last();

                        // SSE clients cannot set headers; allow token in query string
                        if (string.IsNullOrWhiteSpace(token))
                            token = context.Request.Query["access_token"];

                        if (!string.IsNullOrWhiteSpace(token))
                            context.Token = token;

                        return Task.CompletedTask;
                    },
                    OnAuthenticationFailed = context =>
                    {
                        try
                        {
                            var loggerFactory = context.HttpContext.RequestServices.GetService<ILoggerFactory>();
                            var logger = loggerFactory?.CreateLogger("JwtAuth");
                            logger?.LogError(context.Exception, "JWT authentication failed: {Message}", context.Exception.Message);
                        }
                        catch
                        {
                            // ignored
                        }

                        return Task.CompletedTask;
                    },
                    OnChallenge = context =>
                    {
                        try
                        {
                            var loggerFactory = context.HttpContext.RequestServices.GetService<ILoggerFactory>();
                            var logger = loggerFactory?.CreateLogger("JwtAuth");
                            logger?.LogWarning("JWT challenge: {Error} - {ErrorDescription}", context.Error, context.ErrorDescription);
                        }
                        catch
                        {
                            // ignored
                        }

                        return Task.CompletedTask;
                    }
                };
            }
        });

        services.AddAuthorization();
        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
        services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
        services.AddSingleton<MatchLiveHub>();
        services.AddSingleton<IMatchLiveHub>(p => p.GetRequiredService<MatchLiveHub>());
        services.AddSingleton<SportsApi.domain.Abstractions.Live.IMatchLiveHub>(p => p.GetRequiredService<MatchLiveHub>());

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUserService>();

        services.AddHttpClient<IAuthMicroserviceClient, AuthMicroserviceClient>(client =>
        {
            client.BaseAddress = new Uri(
                configuration["SisApi:BaseUrl"] ?? "https://sisapi.microcom.com");
            client.Timeout = TimeSpan.FromSeconds(50);
        });

        var moduleName = configuration["DynamicPermission:Module"] ?? "SIGA";
        services.AddControllers(options =>
        {
            options.Filters.Add(new DynamicPermissionGlobalFilter(moduleName));
        })
        .AddJsonOptions(opts =>
        {
            opts.JsonSerializerOptions.ReferenceHandler =
                System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
            opts.JsonSerializerOptions.IncludeFields = true;
            opts.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
            opts.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.Never;
        });

        return services;
    }
}
