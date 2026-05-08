using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SportsApi.domain.Abstractions.Auth;

namespace SportsApi.infrastructure.Services.Auth;

public class DynamicPermissionGlobalFilter(string module = "SIGA_REALESTATE") : IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var logger = context.HttpContext.RequestServices
            .GetService<ILogger<DynamicPermissionGlobalFilter>>();
        
        var endpoint = context.HttpContext.GetEndpoint();
        if (endpoint?.Metadata.GetMetadata<IAllowAnonymous>() is not null)
            return;
        
        if (HasAttribute<SkipDynamicPermissionAttribute>(context))
            return;
        
        if (context.HttpContext.User.Identity?.IsAuthenticated != true)
        {
            logger?.LogWarning("DynamicPermission: unauthenticated request to {Path}", 
                context.HttpContext.Request.Path);
            context.Result = new UnauthorizedResult();
            return;
        }
        
        var token = context.HttpContext.Request.Headers["Authorization"]
            .ToString().Replace("Bearer ", "");

        if (string.IsNullOrWhiteSpace(token))
            token = context.HttpContext.Request.Cookies["accessToken"] ?? string.Empty;

        if (string.IsNullOrWhiteSpace(token))
        {
            context.Result = new UnauthorizedResult();
            return;
        }
        
        var controllerName = ResolveControllerName(context);
        
        var action = context.HttpContext.Request.Method.ToUpper() switch
        {
            "GET" => "Read",
            "POST" => "Write",
            "PUT" or "PATCH" => "Update",
            "DELETE" => "Delete",
            _ => "Read"
        };
        
        var authClient = context.HttpContext.RequestServices
            .GetRequiredService<IAuthMicroserviceClient>();

        var hasPermission = await authClient.VerifyPermissionAsync(
            token, module, controllerName, action);

        if (!hasPermission)
        {
            logger?.LogWarning(
                "DynamicPermission: denied {Module}-{Controller}:{Action} for user",
                module, controllerName, action);
            context.Result = new ForbidResult(); // 403
        }
    }

    private static string ResolveControllerName(AuthorizationFilterContext context)
    {
        var swaggerTagName = ResolveControllerNameFromSwaggerTag(context);
        if (!string.IsNullOrWhiteSpace(swaggerTagName))
            return swaggerTagName;

        var routeControllerName = context.RouteData.Values["controller"]?.ToString();
        if (!string.IsNullOrWhiteSpace(routeControllerName))
            return routeControllerName;

        if (context.ActionDescriptor is ControllerActionDescriptor descriptor &&
            !string.IsNullOrWhiteSpace(descriptor.ControllerName))
        {
            return descriptor.ControllerName;
        }

        return ResolveEndpointName(context);
    }

    private static string? ResolveControllerNameFromSwaggerTag(AuthorizationFilterContext context)
    {
        if (context.ActionDescriptor is not ControllerActionDescriptor descriptor)
            return null;

        var swaggerOperationAttribute = descriptor.MethodInfo
            .GetCustomAttributes(inherit: true)
            .FirstOrDefault(attribute =>
                string.Equals(
                    attribute.GetType().FullName,
                    "Swashbuckle.AspNetCore.Annotations.SwaggerOperationAttribute",
                    StringComparison.Ordinal));

        if (swaggerOperationAttribute is null)
            return null;

        var tagsProperty = swaggerOperationAttribute.GetType().GetProperty("Tags");
        if (tagsProperty?.GetValue(swaggerOperationAttribute) is not IEnumerable<string> tags)
            return null;

        foreach (var tag in tags)
        {
            if (!string.IsNullOrWhiteSpace(tag))
                return tag.Trim();
        }

        return null;
    }
    
    private static bool HasAttribute<TAttribute>(AuthorizationFilterContext context) where TAttribute : Attribute
    {
        var endpoint = context.HttpContext.GetEndpoint();
        if (endpoint?.Metadata.GetMetadata<TAttribute>() is not null)
            return true;
        
        if (context.ActionDescriptor is ControllerActionDescriptor descriptor)
        {
            if (descriptor.ControllerTypeInfo.GetCustomAttributes(typeof(TAttribute), true).Length > 0)
                return true;
            if (descriptor.MethodInfo.GetCustomAttributes(typeof(TAttribute), true).Length > 0)
                return true;
        }

        return false;
    }
    
    private static string ResolveEndpointName(AuthorizationFilterContext context)
    {
        var path = context.HttpContext.Request.Path.Value ?? string.Empty;
        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length >= 2 && segments[0].Equals("api", StringComparison.OrdinalIgnoreCase))
        {
            return char.ToUpper(segments[1][0]) + segments[1][1..];
        }
        
        if (context.ActionDescriptor is ControllerActionDescriptor descriptor)
        {
            return descriptor.ControllerName;
        }

        return "Unknown";
    }
}