using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using SFA.DAS.Reservations.Infrastructure.Configuration.Configuration;

namespace SFA.DAS.Reservations.Infrastructure.Api
{
    public class ApiClient : IApiClient
    {
        private readonly IOptions<ReservationsApiConfiguration> _apiOptions;

        public ApiClient(IOptions<ReservationsApiConfiguration> apiOptions)
        {
            _apiOptions = apiOptions;
        }

        public async Task<string> GetReservations(long accountId)
        {
            var accessToken = await GetAccessTokenAsync();
            using (var client = new HttpClient())//not unit testable using directly
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var response = await client.GetAsync($"{_apiOptions.Value.Url}api/accounts/{accountId}/reservations").ConfigureAwait(false);

                response.EnsureSuccessStatusCode();

                return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }
        }

        public async Task<string> CreateReservation(long accountId, string json)
        {
            var accessToken = await GetAccessTokenAsync();
            using (var client = new HttpClient())//not unit testable using directly
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var stringContent = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync($"{_apiOptions.Value.Url}api/accounts/{accountId}/reservations", stringContent).ConfigureAwait(false);

                response.EnsureSuccessStatusCode();

                return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }
        }

        private async Task<string> GetAccessTokenAsync()
        {
            var clientCredential = new ClientCredential(_apiOptions.Value.Id, _apiOptions.Value.Secret);
            var context = new AuthenticationContext($"https://login.microsoftonline.com/{_apiOptions.Value.Tenant}", true);

            var result = await context.AcquireTokenAsync(_apiOptions.Value.Identifier, clientCredential).ConfigureAwait(false);

            return result.AccessToken;
        }
    }
}