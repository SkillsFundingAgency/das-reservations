using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using SFA.DAS.Http;
using SFA.DAS.Reservations.Infrastructure.Api;
using SFA.DAS.Reservations.Infrastructure.Extensions;

namespace SFA.DAS.Reservations.Infrastructure.HealthCheck
{
    public class CommitmentsApiHealthCheck : IHealthCheck
    {
        private const string HealthCheckResultDescription = "Commitments Api check";
        private readonly CommitmentsApiClient _commitmentsApiClient;
        private readonly ILogger<CommitmentsApiHealthCheck> _logger;

        public CommitmentsApiHealthCheck(
            CommitmentsApiClient commitmentsApiClient,
            ILogger<CommitmentsApiHealthCheck> logger)
        {
            _logger = logger;
            _commitmentsApiClient = commitmentsApiClient;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
        {
            _logger.LogInformation("Pinging Commitments API");

            try
            {
                var timer = Stopwatch.StartNew();
                await _commitmentsApiClient.Ping();
                timer.Stop();

                var durationString = timer.Elapsed.ToHumanReadableString();

                _logger.LogInformation($"Commitments API ping successful and took {durationString}");

                return HealthCheckResult.Healthy(HealthCheckResultDescription,
                    new Dictionary<string, object> { { "Duration", durationString } });
            }
            catch (RestHttpClientException e)
            {
                _logger.LogWarning($"Commitments API ping failed : [Code: {e.StatusCode}] - {e.ReasonPhrase}");
                return HealthCheckResult.Unhealthy(HealthCheckResultDescription, e);
            }
        }
    }
}
