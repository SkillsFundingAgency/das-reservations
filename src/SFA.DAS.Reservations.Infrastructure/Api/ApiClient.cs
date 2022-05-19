using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using SFA.DAS.Reservations.Infrastructure.Configuration;

namespace SFA.DAS.Reservations.Infrastructure.Api
{
    public class ApiClient : ApiClientBase, IApiClient
    {
        private readonly ReservationsApiConfiguration _reservationsApiClientConfiguration;

        public ApiClient(IOptions<ReservationsApiConfiguration> apiConfigurationOptions)
        {
            _reservationsApiClientConfiguration = apiConfigurationOptions.Value;
        }

        public override async Task<string> Ping()
        {
            var pingUrl = _reservationsApiClientConfiguration.Url;

            pingUrl += pingUrl.EndsWith("/") ? "ping" : "/ping";

            using (var client = new HttpClient())//not unit testable using directly
            {
                var response = await client.GetAsync(pingUrl).ConfigureAwait(false);

                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return result;
            }
        }

        protected override async Task<string> GetAccessTokenAsync()
        {
            var accessToken = IsClientCredentialConfiguration(_reservationsApiClientConfiguration.Id, _reservationsApiClientConfiguration.Secret, _reservationsApiClientConfiguration.Tenant)
               ? await GetClientCredentialAuthenticationResult(_reservationsApiClientConfiguration.Id, _reservationsApiClientConfiguration.Secret, _reservationsApiClientConfiguration.Identifier, _reservationsApiClientConfiguration.Tenant)
               : await GetManagedIdentityAuthenticationResult(_reservationsApiClientConfiguration.Identifier);

            return accessToken;
        }

        private async Task<string> GetClientCredentialAuthenticationResult(string clientId, string clientSecret, string resource, string tenant)
        {
            var authority = $"https://login.microsoftonline.com/{tenant}";
            var clientCredential = new ClientCredential(clientId, clientSecret);
            var context = new AuthenticationContext(authority, true);
            var result = await context.AcquireTokenAsync(resource, clientCredential);
            return result.AccessToken;
        }

        private async Task<string> GetManagedIdentityAuthenticationResult(string resource)
        {
            var azureServiceTokenProvider = new AzureServiceTokenProvider();
            return await azureServiceTokenProvider.GetAccessTokenAsync(resource);
        }

        private bool IsClientCredentialConfiguration(string clientId, string clientSecret, string tenant)
        {
            return !string.IsNullOrEmpty(clientId) && !string.IsNullOrEmpty(clientSecret) && !string.IsNullOrEmpty(tenant);
        }
    }
}