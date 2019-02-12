using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;
using SFA.DAS.Reservations.Domain.ReservationsApi;
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

        public async Task<IEnumerable<TResponse>> Get<TRequest, TResponse>(TRequest request) where TRequest : BaseApiRequest
        {
            var accessToken = await GetAccessTokenAsync();
            using (var client = new HttpClient())//not unit testable using directly
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var response = await client.GetAsync(request.GetUrl).ConfigureAwait(false);

                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return JsonConvert.DeserializeObject<IEnumerable<TResponse>>(json);
            }
        }

        public async Task<TResponse> Create<TRequest, TResponse>(TRequest request) where TRequest : BaseApiRequest
        {
            var accessToken = await GetAccessTokenAsync();
            using (var client = new HttpClient())//not unit testable using directly
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var jsonRequest = JsonConvert.SerializeObject(request);
                var stringContent = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
                
                var response = await client.PostAsync(request.CreateUrl, stringContent).ConfigureAwait(false);

                response.EnsureSuccessStatusCode();
                var jsonResponse = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return JsonConvert.DeserializeObject<TResponse>(jsonResponse);
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