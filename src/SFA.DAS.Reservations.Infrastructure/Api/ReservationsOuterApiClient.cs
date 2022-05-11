using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Infrastructure.Configuration;

namespace SFA.DAS.Reservations.Infrastructure.Api
{
    public class ReservationsOuterApiClient : IReservationsOuterApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly AsyncRetryPolicy _asyncRetryPolicy;
        private readonly ReservationsOuterApiConfiguration _config;
        private readonly ILogger<ReservationsOuterApiClient> _logger;

        public ReservationsOuterApiClient (HttpClient httpClient, ReservationsOuterApiConfiguration config, ILogger<ReservationsOuterApiClient> logger)
        {
            _config = config;
            _logger = logger;
            _httpClient = httpClient;
            _asyncRetryPolicy = GetRetryPolicy();

            AddHeaders();
        }

        public async Task<TResponse> Get<TResponse>(IGetApiRequest request) 
        {

            _logger.LogInformation("Calling Outer API base {0}, url {1}", _config.ApiBaseUrl, request.GetUrl);
            var response = await _httpClient.GetAsync(request.GetUrl).ConfigureAwait(false);

            if (response.StatusCode.Equals(HttpStatusCode.NotFound))
            {
                _logger.LogInformation("URL {0} found nothing", request.GetUrl);
                return default;
            }

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("URL {0} returned a response", request.GetUrl);
                var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return JsonConvert.DeserializeObject<TResponse>(json);    
            }

            _logger.LogInformation("URL {0} returned a response {1}", request.GetUrl, response.StatusCode);
            response.EnsureSuccessStatusCode();
            
            return default;
        }

        public async Task<TResponse> GetWithRetry<TResponse>(IGetApiRequest request)
        {
            return await _asyncRetryPolicy.ExecuteAsync(async() => await Get<TResponse>(request));
        }

        private void AddHeaders()
        {
            _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _config.SubscriptionKey);
            _httpClient.DefaultRequestHeaders.Add("X-Version", "1");
        }

        private AsyncRetryPolicy GetRetryPolicy()
        {
            var maxRetryAttempts = 3;
            var pauseBetweenFailures = TimeSpan.FromSeconds(2);

            return Policy
                .Handle<HttpRequestException>()
                .WaitAndRetryAsync(maxRetryAttempts, i => pauseBetweenFailures);
        }
    }
}