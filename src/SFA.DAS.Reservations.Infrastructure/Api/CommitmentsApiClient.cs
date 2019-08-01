using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using SFA.DAS.Reservations.Infrastructure.Configuration;

namespace SFA.DAS.Reservations.Infrastructure.Api
{
    public class CommitmentsApiClient : ApiClientBase, IApiClient
    {
        private readonly IOptions<CommitmentsApiConfiguration> _apiOptions;

        public CommitmentsApiClient(IOptions<CommitmentsApiConfiguration> apiOptions)
        {
            _apiOptions = apiOptions;
        }

        public override async Task<string> Ping()
        {
            var pingUrl = _apiOptions.Value.ApiBaseUrl;

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
            var clientCredential = new ClientCredential(_apiOptions.Value.ClientId, _apiOptions.Value.ClientSecret);
            var context = new AuthenticationContext($"https://login.microsoftonline.com/{_apiOptions.Value.Tenant}", true);

            var result = await context.AcquireTokenAsync(_apiOptions.Value.IdentifierUri, clientCredential).ConfigureAwait(false);

            return result.AccessToken;
        }
    }
}
