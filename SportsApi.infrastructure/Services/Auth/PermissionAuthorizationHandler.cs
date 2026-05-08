using Microsoft.AspNetCore.Authorization;

namespace SportsApi.infrastructure.Services.Auth;

public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        if (context.User.Identity?.IsAuthenticated != true)
            return Task.CompletedTask;

        var permissions = context.User.Claims
            .Where(c => c.Type == "Permission")
            .Select(c => c.Value);

        if (permissions.Contains(requirement.Permission))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}