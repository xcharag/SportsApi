using Microsoft.AspNetCore.Authorization;

namespace SportsApi.infrastructure.Services.Auth;

public class PermissionRequirement(string permission) : IAuthorizationRequirement
{
    public string Permission { get; } = permission;
}
