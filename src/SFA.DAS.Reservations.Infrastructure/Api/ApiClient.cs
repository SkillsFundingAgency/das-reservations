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
        private readonly IOptions<ReservationsApiConfiguration> _apiOptions;

        public ApiClient(IOptions<ReservationsApiConfiguration> apiOptions)
        {
            _apiOptions = apiOptions;
        }

        public override async Task<string> Ping()
        {
            var pingUrl = _apiOptions.Value.Url;

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
            var azureServiceTokenProvider = new AzureServiceTokenProvider();
            string accessToken = await azureServiceTokenProvider.GetAccessTokenAsync(_apiOptions.Value.Identifier);
            return accessToken;
        }
    }
}