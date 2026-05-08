using Microsoft.AspNetCore.Authorization;

namespace SportsApi.infrastructure.Services.Auth;

/// <summary>
/// Explicit/static permission check for fine-grained control on individual actions.
/// Usage: [RequirePermission("SportsApi", "Clients", "Read")]
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class RequirePermissionAttribute : AuthorizeAttribute
{
    public const string PolicyPrefix = "Permission_";

    public RequirePermissionAttribute(string module, string controller, string action)
    {
        Policy = $"{PolicyPrefix}{module}-{controller}:{action}";
    }
}