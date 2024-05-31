using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using SFA.DAS.Http;
using SFA.DAS.Reservations.Domain.ProviderRelationships.Api;
using SFA.DAS.Reservations.Infrastructure.Api;
using SFA.DAS.Reservations.Infrastructure.Extensions;

namespace SFA.DAS.Reservations.Infrastructure.HealthCheck
{
    public class ProviderRelationshipsApiHealthCheck : IHealthCheck
    {
        private const string HealthCheckResultDescription = "ProviderRelationships Api check";
        private readonly IProviderRelationshipsOuterApiClient _apiClient;
        private readonly ILogger<ProviderRelationshipsApiHealthCheck> _logger;

        public ProviderRelationshipsApiHealthCheck(
            IProviderRelationshipsOuterApiClient apiClient,
            ILogger<ProviderRelationshipsApiHealthCheck> logger)
        {
            _apiClient = apiClient;
            _logger = logger;
        }
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
        {
            _logger.LogInformation("Pinging ProviderRelationships API");

            try
            {
                var timer = Stopwatch.StartNew();
                await _apiClient.Get<GetAccountProviderLegalEntitiesWithPermissionRequest>(new GetAccountProviderLegalEntitiesWithPermissionRequest());
                timer.Stop();

                var durationString = timer.Elapsed.ToHumanReadableString();

                _logger.LogInformation($"ProviderRelationships API ping successful and took {durationString}");

                return HealthCheckResult.Healthy(HealthCheckResultDescription,
                    new Dictionary<string, object> { { "Duration", durationString } });
            }
            catch (RestHttpClientException e)
            {
                _logger.LogWarning($"ProviderRelationships API ping failed : [Code: {e.StatusCode}] - {e.ReasonPhrase}");
                return HealthCheckResult.Unhealthy(HealthCheckResultDescription, e);
            }
        }
    }
}
