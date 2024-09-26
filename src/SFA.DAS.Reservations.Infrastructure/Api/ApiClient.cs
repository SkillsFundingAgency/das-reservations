using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using SFA.DAS.Reservations.Infrastructure.Configuration;

namespace SFA.DAS.Reservations.Infrastructure.Api;

public class ApiClient(IOptions<ReservationsApiConfiguration> apiConfigurationOptions) : ApiClientBase, IApiClient
{
    private readonly ReservationsApiConfiguration _reservationsApiClientConfiguration = apiConfigurationOptions.Value;
    // Make use of built-in token caching with AzureServiceTokenProvider.
    private static readonly AzureServiceTokenProvider TokenProvider = new();

    public override async Task<string> Ping()
    {
        var pingUrl = _reservationsApiClientConfiguration.Url;

        pingUrl += pingUrl.EndsWith("/") ? "ping" : "/ping";

        //not unit testable using directly
        using var client = new HttpClient();
        
        var response = await client.GetAsync(pingUrl).ConfigureAwait(false);

        response.EnsureSuccessStatusCode();
        
        return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
    }

    protected override async Task<string> GetAccessTokenAsync()
    {
        var accessToken = IsClientCredentialConfiguration(_reservationsApiClientConfiguration.Id, _reservationsApiClientConfiguration.Secret, _reservationsApiClientConfiguration.Tenant)
            ? await GetClientCredentialAuthenticationResult(_reservationsApiClientConfiguration.Id, _reservationsApiClientConfiguration.Secret, _reservationsApiClientConfiguration.Identifier, _reservationsApiClientConfiguration.Tenant)
            : await GetManagedIdentityAuthenticationResult(_reservationsApiClientConfiguration.Identifier);

        return accessToken;
    }

    private static async Task<string> GetClientCredentialAuthenticationResult(string clientId, string clientSecret, string resource, string tenant)
    {
        var authority = $"https://login.microsoftonline.com/{tenant}";
        var clientCredential = new ClientCredential(clientId, clientSecret);
        var context = new AuthenticationContext(authority, true);
        var result = await context.AcquireTokenAsync(resource, clientCredential);
        return result.AccessToken;
    }

    private static async Task<string> GetManagedIdentityAuthenticationResult(string resource)
    {
        return await TokenProvider.GetAccessTokenAsync(resource);
    }

    private static bool IsClientCredentialConfiguration(string clientId, string clientSecret, string tenant)
    {
        return !string.IsNullOrEmpty(clientId) && !string.IsNullOrEmpty(clientSecret) && !string.IsNullOrEmpty(tenant);
    }
}