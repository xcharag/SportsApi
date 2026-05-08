using System.Net.Http.Headers;
using System.Text.Json;
using SportsApi.domain.Abstractions.Auth;

namespace SportsApi.infrastructure.Services.Auth;

public class AuthMicroserviceClient(HttpClient httpClient) : IAuthMicroserviceClient
{
    public async Task<bool> VerifyPermissionAsync(
        string token, string module, string controller, string action, int typePermission = 0)
    {
        try
        {
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var url = $"/api/Auth/verify-permission?module={module}" +
                      $"&controller={controller}&action={action}&typePermission={typePermission}";

            var response = await httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<PermissionVerificationResponse>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return result?.Success == true && result?.Data == true;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }
}

public class PermissionVerificationResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public bool Data { get; set; }
    public object? Errors { get; set; }
}