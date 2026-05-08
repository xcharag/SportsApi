namespace SportsApi.domain.Abstractions.Auth;

public interface IAuthMicroserviceClient
{
    Task<bool> VerifyPermissionAsync(
        string token, string module, string controller, string action, int typePermission = 0);
}