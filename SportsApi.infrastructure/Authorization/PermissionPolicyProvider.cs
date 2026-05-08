using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using SportsApi.infrastructure.Services.Auth;

namespace SportsApi.infrastructure.Authorization;

public class PermissionPolicyProvider(IOptions<AuthorizationOptions> options) : IAuthorizationPolicyProvider
{
    private readonly DefaultAuthorizationPolicyProvider _fallbackPolicyProvider = new(options);

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync() => 
        _fallbackPolicyProvider.GetDefaultPolicyAsync();

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() => 
        _fallbackPolicyProvider.GetFallbackPolicyAsync();

    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (!policyName.StartsWith(RequirePermissionAttribute.PolicyPrefix, StringComparison.OrdinalIgnoreCase))
            return _fallbackPolicyProvider.GetPolicyAsync(policyName);
        var permission = policyName.Substring(RequirePermissionAttribute.PolicyPrefix.Length);
            
        var policy = new AuthorizationPolicyBuilder()
            .AddRequirements(new PermissionRequirement(permission))
            .Build();
            
        return Task.FromResult<AuthorizationPolicy?>(policy);

    }
}