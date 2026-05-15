using System.Diagnostics;
using System.Net;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportsApi.application;
using SportsApi.domain.Abstractions.Auth;
using SportsApi.domain.Abstractions.Exceptions;
using SportsApi.infrastructure;
using SportsApi.infrastructure.Persistence.Core.EntityFramework;
using SportsApi.infrastructure.Services.Auth;
using Swashbuckle.AspNetCore.Annotations;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.IgnoreReadOnlyProperties = false;
        options.JsonSerializerOptions.IgnoreReadOnlyFields = false;
        // Forces all incoming DateTime values to DateTimeKind.Utc so Npgsql can write
        // them to "timestamp with time zone" columns without throwing ArgumentException.
        options.JsonSerializerOptions.Converters.Add(new SportsApi.api.Infrastructure.UtcDateTimeConverter());
    });

var sisApiBase = builder.Configuration["SisApi:BaseUrl"] ?? "https://sisapi.microcom-ti.com";

builder.Services.AddHttpClient<IAuthMicroserviceClient, AuthMicroserviceClient>(client =>
{
    client.BaseAddress = new Uri(sisApiBase);
    client.Timeout = TimeSpan.FromSeconds(120);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
})
.ConfigurePrimaryHttpMessageHandler(() =>
{
    var handler = new HttpClientHandler();
    if (builder.Environment.IsDevelopment())
    {
        handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
    }
    return handler;
});

string[] allowedOrigins;
var corsSection = builder.Configuration.GetSection("CorsOrigins").Get<string[]>();
if (corsSection != null && corsSection.Length > 0)
{
    allowedOrigins = corsSection.Select(s => s?.Trim())
        .Where(s => !string.IsNullOrWhiteSpace(s))
        .Distinct()
        .ToArray()!;
}
else
{
    var allowedOriginsStr = builder.Configuration["AllowedOrigins"];
    allowedOrigins = string.IsNullOrWhiteSpace(allowedOriginsStr)
        ? Array.Empty<string>()
        : allowedOriginsStr.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim())
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Distinct()
            .ToArray();
}

builder.Services.AddCors(options =>
{
    options.AddPolicy("ProductionPolicy", policy =>
    {
        if (allowedOrigins.Length > 0)
        {
            policy.WithOrigins(allowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials()
                .WithExposedHeaders("Content-Disposition", "Content-Length")
                .SetPreflightMaxAge(TimeSpan.FromMinutes(10));
        }
        else
        {
            policy.AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
        }
    });

    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy.WithOrigins("https://localhost:5173", "http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .WithExposedHeaders("Content-Disposition", "Content-Length")
            .SetPreflightMaxAge(TimeSpan.FromMinutes(10));
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.EnableAnnotations();
    options.TagActionsBy(api =>
    {
        if (api.ActionDescriptor is Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor descriptor)
        {
            var attr = descriptor.MethodInfo
                .GetCustomAttributes(typeof(SwaggerOperationAttribute), true)
                .OfType<SwaggerOperationAttribute>()
                .FirstOrDefault();

            if (attr?.Tags?.Length > 0)
                return attr.Tags;
        }
        return new[] { api.ActionDescriptor.RouteValues["controller"] ?? "Core" };
    });

    options.CustomSchemaIds(type => type.FullName?.Replace('+', '.') ?? type.Name);

    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter your JWT token obtained from SISAPI login.\nExample: eyJhbGciOiJIUzI1NiIs..."
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exceptionFeature = context.Features.Get<IExceptionHandlerFeature>();
        var exception = exceptionFeature?.Error;

        if (exception is DomainConflictException conflict)
        {
            context.Response.StatusCode = (int)HttpStatusCode.Conflict;
            context.Response.ContentType = "application/problem+json";

            var problem = new ProblemDetails
            {
                Title = "Conflicto de unicidad",
                Detail = conflict.Message,
                Status = (int)HttpStatusCode.Conflict,
                Type = "https://httpstatuses.com/409"
            };
            problem.Extensions["errorKey"] = conflict.ErrorKey;
            problem.Extensions["traceId"] = Activity.Current?.Id ?? context.TraceIdentifier;

            await context.Response.WriteAsJsonAsync(problem);
            return;
        }

        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        context.Response.ContentType = "application/problem+json";
        var generic = new ProblemDetails
        {
            Title = "Internal Server Error",
            Detail = "Ocurrio un error inesperado.",
            Status = (int)HttpStatusCode.InternalServerError,
            Type = "https://httpstatuses.com/500"
        };
        generic.Extensions["traceId"] = Activity.Current?.Id ?? context.TraceIdentifier;

        await context.Response.WriteAsJsonAsync(generic);
    });
});

var logger = app.Services.GetRequiredService<ILogger<Program>>();
if (allowedOrigins.Length > 0)
    logger.LogInformation("CORS allowed origins: {Origins}", string.Join(",", allowedOrigins));
else
    logger.LogInformation("CORS: no explicit origins configured, using AllowAnyOrigin().");

var corsPolicy = app.Environment.IsDevelopment() ? "FrontendPolicy" : "ProductionPolicy";
logger.LogInformation("Active CORS Policy: {Policy}", corsPolicy);
app.UseCors(corsPolicy);

using (var scope = app.Services.CreateScope())
{
    var migrationLogger = scope.ServiceProvider
        .GetRequiredService<ILoggerFactory>()
        .CreateLogger("Migration");
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<CoreDbContext>();
        migrationLogger.LogInformation("Applying database migrations...");
        db.Database.Migrate();
        migrationLogger.LogInformation("Database migrations applied successfully.");
    }
    catch (Exception ex)
    {
        migrationLogger.LogError(ex, "An error occurred while applying database migrations.");
        throw;
    }
}

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/health", () => Results.Ok(new
{
    status = "healthy",
    timestamp = DateTime.UtcNow
})).AllowAnonymous();

var useHttpsRedirection = builder.Configuration.GetValue("UseHttpsRedirection", false);
if (useHttpsRedirection)
    app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();