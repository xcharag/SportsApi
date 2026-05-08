namespace SportsApi.infrastructure.Services.Auth;

/// <summary>
/// Place this attribute on an Ardalis endpoint class or action method to skip 
/// the automatic dynamic permission check performed by the global filter.
/// Endpoints decorated with this attribute will still require a valid JWT 
/// unless [AllowAnonymous] is also applied.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class SkipDynamicPermissionAttribute : Attribute;

/// <summary>
/// Alias for <see cref="SkipDynamicPermissionAttribute"/>.
/// Place this attribute on an Ardalis endpoint class or action method to skip
/// the automatic dynamic permission check performed by the global filter.
/// Endpoints decorated with this attribute will still require a valid JWT
/// unless [AllowAnonymous] is also applied.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public sealed class SkipDynamicAuthorizationAttribute : SkipDynamicPermissionAttribute;
