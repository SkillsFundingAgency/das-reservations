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

namespace SFA.DAS.Reservations.Infrastructure.Api;
public class ProviderRelationshipsOuterApiClient : IProviderRelationshipsOuterApiClient
{
    private readonly HttpClient _httpClient;
    private readonly AsyncRetryPolicy _asyncRetryPolicy;
    private readonly ProviderRelationshipsOuterApiConfiguration _config;
    private readonly ILogger<IProviderRelationshipsOuterApiClient> _logger;

    public ProviderRelationshipsOuterApiClient(HttpClient httpClient, ProviderRelationshipsOuterApiConfiguration config, ILogger<IProviderRelationshipsOuterApiClient> logger)
    {
        _httpClient = httpClient;
        _asyncRetryPolicy = GetRetryPolicy();
        _config = config;
        _logger = logger;
    }

    public async Task<TResponse> Get<TResponse>(IGetApiRequest request)
    {
        _logger.LogInformation("Calling Outer API base");

        var httpMessage = new HttpRequestMessage(HttpMethod.Get, request.GetUrl);

        AddHeaders(httpMessage);

        var response = await _httpClient.SendAsync(httpMessage).ConfigureAwait(false);

        if (response.StatusCode.Equals(HttpStatusCode.NotFound))
        {
            _logger.LogInformation("Found nothing");
            return default;
        }

        if (response.IsSuccessStatusCode)
        {
            _logger.LogInformation("Returned a response");
            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonConvert.DeserializeObject<TResponse>(json);
        }

        _logger.LogInformation("Returned a response {0}", response.StatusCode);
        response.EnsureSuccessStatusCode();

        return default;
    }
    private void AddHeaders(HttpRequestMessage httpRequestMessage)
    {
        httpRequestMessage.Headers.Add("Ocp-Apim-Subscription-Key", _config.SubscriptionKey);
        httpRequestMessage.Headers.Add("X-Version", "1");
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
