using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SFA.DAS.Reservations.Domain.Interfaces;

namespace SFA.DAS.Reservations.Infrastructure.Api
{
    public abstract class ApiClientBase
    {
        public async Task<TResponse> Get<TResponse>(IGetApiRequest request) 
        {
            var accessToken = await GetAccessTokenAsync();
            using (var client = new HttpClient())//not unit testable using directly
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var response = await client.GetAsync(request.GetUrl).ConfigureAwait(false);

                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return JsonConvert.DeserializeObject<TResponse>(json);
            }
        }

        public async Task<IEnumerable<TResponse>> GetAll<TResponse>(IGetAllApiRequest request)
        {
            var accessToken = await GetAccessTokenAsync();
            using (var client = new HttpClient())//not unit testable using directly
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var response = await client.GetAsync(request.GetAllUrl).ConfigureAwait(false);

                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return JsonConvert.DeserializeObject<IEnumerable<TResponse>>(json);
            }
        }

        public async Task<TResponse> Create<TResponse>(IPostApiRequest request)
        {
            var accessToken = await GetAccessTokenAsync();
            using (var client = new HttpClient())//not unit testable using directly
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var jsonRequest = JsonConvert.SerializeObject(request);
                var stringContent = new StringContent(jsonRequest, System.Text.Encoding.UTF8, "application/json");
                
                var response = await client.PostAsync(request.CreateUrl, stringContent).ConfigureAwait(false);

                response.EnsureSuccessStatusCode();
                var jsonResponse = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return JsonConvert.DeserializeObject<TResponse>(jsonResponse);
            }
        }

        public async Task Delete(IDeleteApiRequest request)
        {
            var accessToken = await GetAccessTokenAsync();
            using (var client = new HttpClient())//not unit testable using directly
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var jsonRequest = JsonConvert.SerializeObject(request);
                var stringContent = new StringContent(jsonRequest, System.Text.Encoding.UTF8, "application/json");
                
                var response = await client.DeleteAsync(request.DeleteUrl).ConfigureAwait(false);

                response.EnsureSuccessStatusCode();
            }
        }

        public abstract Task<string> Ping();

        protected abstract Task<string> GetAccessTokenAsync();
    }
}
