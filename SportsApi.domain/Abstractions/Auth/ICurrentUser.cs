namespace SportsApi.domain.Abstractions.Auth;

/// <summary>
/// Provides access to the currently authenticated user's claims extracted from the JWT token.
/// </summary>
public interface ICurrentUser
{
    /// <summary>
    /// Gets the unique identifier of the current user (NameIdentifier claim).
    /// </summary>
    string UserId { get; }

    /// <summary>
    /// Gets the username of the current user (Name claim).
    /// </summary>
    string Username { get; }

    /// <summary>
    /// Gets the email address of the current user (EmailAddress claim).
    /// </summary>
    string Email { get; }

    /// <summary>
    /// Gets the unique token ID (jti claim).
    /// </summary>
    string TokenId { get; }

    /// <summary>
    /// Gets the company ID from the JWT token (custom claim).
    /// </summary>
    string CompanyId { get; }

    /// <summary>
    /// Gets a list of roles assigned to the current user.
    /// </summary>
    IReadOnlyList<string> Roles { get; }

    /// <summary>
    /// Gets the token expiration time in Unix seconds (exp claim).
    /// </summary>
    long ExpiresAt { get; }

    /// <summary>
    /// Gets the issuer of the token (iss claim).
    /// </summary>
    string Issuer { get; }

    /// <summary>
    /// Gets the audience of the token (aud claim).
    /// </summary>
    string Audience { get; }

    /// <summary>
    /// Checks if the current user is authenticated.
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// Returns the JWT authentication error message if token validation failed, or null if no error.
    /// Useful for diagnosing secret key mismatches, expired tokens, invalid issuer/audience, etc.
    /// </summary>
    string? AuthenticationError { get; }
}