using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SportsApi.domain.Abstractions.Auth;
using SportsApi.domain.Enums;

namespace SportsApi.infrastructure.Authorization;

public class DynamicPermissionFilter(Module module = Module.SportsApi, int typePermission = 0) : IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        if (context.HttpContext.User.Identity?.IsAuthenticated != true)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var authClient = context.HttpContext.RequestServices.GetRequiredService<IAuthMicroserviceClient>();
        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<DynamicPermissionFilter>>();

        var token = context.HttpContext.Request.Cookies["accessToken"];
        if (string.IsNullOrEmpty(token))
        {
            var authHeader = context.HttpContext.Request.Headers["Authorization"].ToString();
            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                token = authHeader.Substring("Bearer ".Length).Trim();
            }
        }

        if (string.IsNullOrEmpty(token))
        {
            logger.LogWarning("No accessToken cookie or Authorization header found in request");
            context.Result = new UnauthorizedResult();
            return;
        }
        string resourceFromTag = string.Empty;
        var endpoint = context.HttpContext.GetEndpoint();
        if (endpoint != null)
        {
            var swagMeta = endpoint.Metadata.FirstOrDefault(m => string.Equals(m.GetType().Name, "SwaggerOperationAttribute", StringComparison.Ordinal));
            if (swagMeta != null)
            {
                try
                {
                    var tagsProp = swagMeta.GetType().GetProperty("Tags");
                    if (tagsProp != null)
                    {
                        var tagsObj = tagsProp.GetValue(swagMeta);
                        if (tagsObj is System.Collections.IEnumerable en)
                        {
                            foreach (var t in en)
                            {
                                if (t != null)
                                {
                                    var ts = t.ToString();
                                    if (!string.IsNullOrWhiteSpace(ts))
                                    {
                                        resourceFromTag = ts; // take the tag exactly as defined
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
                catch { /* ignore reflection errors and fallback */ }
            }
        }

        string resourceFromController = context.RouteData.Values["controller"]?.ToString() ?? string.Empty;
        string routeSegment = ResolveEndpointName(context); // raw segment from path if any

        var httpMethod = context.HttpContext.Request.Method.ToUpperInvariant();
        var action = httpMethod switch
        {
            "GET" => "Read",
            "POST" => "Write",
            "PUT" => "Update",
            "PATCH" => "Update",
            "DELETE" => "Delete",
            _ => "Read"
        };

        // Build candidates in order of preference, using exact strings (no casing or normalization changes)
        var candidates = new[] { resourceFromTag, resourceFromController, routeSegment }
            .Where(s => !string.IsNullOrEmpty(s))
            .Select(s => s.Trim())
            .Distinct(StringComparer.Ordinal)
            .ToList();

        logger.LogDebug("DynamicPermission: candidates to verify (exact): {Candidates}", string.Join(", ", candidates));

        bool hasPermission = false;
        foreach (var candidate in candidates)
        {
            logger.LogDebug("DynamicPermission: verifying exact candidate '{Candidate}' for action {Action}", candidate, action);
            hasPermission = await authClient.VerifyPermissionAsync(
                token,
                module.ToString(),
                candidate,
                action,
                typePermission
            );

            logger.LogDebug("DynamicPermission: exact candidate '{Candidate}' returned {Result}", candidate, hasPermission);
            if (hasPermission) break;
        }

        if (!hasPermission)
        {
            logger.LogWarning("Permission denied for: {Module}-<exact candidates>:{Action} (candidates tried: {Candidates})", module, action, string.Join(", ", candidates));
            context.Result = new ForbidResult();
            return;
        }

        logger.LogInformation("Permission granted for: {Module}-{Resource}:{Action}", module, candidates.FirstOrDefault(), action);
    }

    private static string ResolveEndpointName(AuthorizationFilterContext context)
    {
        var path = context.HttpContext.Request.Path.Value ?? string.Empty;
        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length >= 2 && segments[0].Equals("api", StringComparison.OrdinalIgnoreCase))
        {
            return segments[1];
        }

        if (context.ActionDescriptor is Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor descriptor)
        {
            return descriptor.ControllerName;
        }

        return string.Empty;
    }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class DynamicPermissionAttribute : TypeFilterAttribute
{
    public DynamicPermissionAttribute(Module module = Module.SportsApi, int typePermission = 0)
        : base(typeof(DynamicPermissionFilter))
    {
        Arguments = new object[] { module, typePermission };
    }
}