using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SportsApi.domain.Abstractions.Auth;

namespace SportsApi.infrastructure.Services.Auth;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    private ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;

    public string UserId => GetRequiredClaimValue(ClaimTypes.NameIdentifier);

    public string Username => GetRequiredClaimValue(ClaimTypes.Name);

    public string Email => GetClaimValue(ClaimTypes.Email) ?? string.Empty;

    public string TokenId => GetClaimValue("jti") ?? string.Empty;

    public string CompanyId => GetClaimValue("CompanyId") ?? string.Empty;

    public IReadOnlyList<string> Roles
    {
        get
        {
            var roleClaims = User?.FindAll(ClaimTypes.Role) ?? [];
            return roleClaims.Select(c => c.Value).ToList().AsReadOnly();
        }
    }

    public long ExpiresAt
    {
        get
        {
            var expClaim = GetClaimValue("exp");
            return long.TryParse(expClaim, out var expValue) ? expValue : 0;
        }
    }

    public string Issuer => GetClaimValue("iss") ?? string.Empty;

    public string Audience => GetClaimValue("aud") ?? string.Empty;

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

    /// <summary>
    /// Returns the JWT authentication error stored during token validation, or null if none.
    /// </summary>
    public string? AuthenticationError
    {
        get
        {
            var httpContext = httpContextAccessor.HttpContext;
            if (httpContext != null &&
                httpContext.Items.TryGetValue("JwtAuthError", out var error) &&
                error is string errorMsg)
            {
                return errorMsg;
            }
            return null;
        }
    }

    private string? GetClaimValue(string claimType)
    {
        return User?.FindFirst(claimType)?.Value;
    }

    /// <summary>
    /// Returns the claim value, or — when the user is NOT authenticated —
    /// checks whether JWT validation failed and throws with the real reason.
    /// </summary>
    private string GetRequiredClaimValue(string claimType)
    {
        var value = User?.FindFirst(claimType)?.Value;
        if (!string.IsNullOrEmpty(value))
            return value;

        // User is not authenticated — check if there was a JWT error
        if (User?.Identity?.IsAuthenticated != true)
        {
            var jwtError = AuthenticationError;
            if (!string.IsNullOrEmpty(jwtError))
            {
                throw new UnauthorizedAccessException(
                    $"Cannot read claim '{claimType}' because JWT authentication failed: {jwtError}");
            }

            // Token was never sent
            var httpContext = httpContextAccessor.HttpContext;
            var hasAuthHeader = httpContext?.Request.Headers.ContainsKey("Authorization") == true;
            var hasCookie = httpContext?.Request.Cookies.ContainsKey("accessToken") == true;

            if (hasAuthHeader || hasCookie)
            {
                throw new UnauthorizedAccessException(
                    $"Cannot read claim '{claimType}'. A token was provided but the user is not authenticated. " +
                    "This usually means the JWT SecretKey, Issuer, or Audience in appsettings.json " +
                    "does not match the values used by the auth microservice (SISAPI).");
            }

            // No token at all
            throw new UnauthorizedAccessException(
                $"Cannot read claim '{claimType}'. No JWT token was provided in the request. " +
                "Send a Bearer token in the Authorization header or an accessToken cookie.");
        }

        // Authenticated but claim is missing
        return string.Empty;
    }
}

