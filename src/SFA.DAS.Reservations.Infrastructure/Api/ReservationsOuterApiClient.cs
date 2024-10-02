using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Infrastructure.Configuration;

namespace SFA.DAS.Reservations.Infrastructure.Api;

public class ReservationsOuterApiClient(
    HttpClient httpClient, 
    ReservationsOuterApiConfiguration config,
    ILogger<ReservationsOuterApiClient> logger)
    : IReservationsOuterApiClient
{
    public async Task<TResponse> Get<TResponse>(IGetApiRequest request)
    {
        logger.LogInformation("Calling Outer API base");

        using var httpMessage = new HttpRequestMessage(HttpMethod.Get, request.GetUrl);

        AddHeaders(httpMessage);

        var response = await httpClient.SendAsync(httpMessage).ConfigureAwait(false);

        if (response.StatusCode.Equals(HttpStatusCode.NotFound))
        {
            logger.LogInformation("Found nothing");
            return default;
        }

        if (response.IsSuccessStatusCode)
        {
            logger.LogInformation("Returned a response");
            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonConvert.DeserializeObject<TResponse>(json);
        }

        logger.LogInformation("Returned a response {StatusCode}", response.StatusCode);
        response.EnsureSuccessStatusCode();

        return default;
    }

    private void AddHeaders(HttpRequestMessage httpRequestMessage)
    {
        httpRequestMessage.Headers.Add("Ocp-Apim-Subscription-Key", config.SubscriptionKey);
        httpRequestMessage.Headers.Add("X-Version", "1");
    }
}