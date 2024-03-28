using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using SFA.DAS.EAS.Account.Api.Client;
using SFA.DAS.Http;
using SFA.DAS.Reservations.Infrastructure.Extensions;

namespace SFA.DAS.Reservations.Infrastructure.HealthCheck
{
     public class AccountApiHealthCheck : IHealthCheck
     {
        private const string HealthCheckResultDescription = "Account Api check";
        private readonly IAccountApiClient _apiClient;
        private readonly ILogger<AccountApiHealthCheck> _logger;

        public AccountApiHealthCheck(
            IAccountApiClient apiClient,
            ILogger<AccountApiHealthCheck> logger)
        {
            _apiClient = apiClient;
            _logger = logger;
        }
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
        {
            _logger.LogInformation("Pinging Accounts API");

            try
            {
                var timer = Stopwatch.StartNew();
                await _apiClient.GetAccount(1);
                timer.Stop();

                var durationString = timer.Elapsed.ToHumanReadableString();

                _logger.LogInformation($"Account API ping successful and took {durationString}");

                return HealthCheckResult.Healthy(HealthCheckResultDescription,
                    new Dictionary<string, object> { { "Duration", durationString } });
            }
            catch (RestHttpClientException e)
            {
                _logger.LogWarning($"Account API ping failed : [Code: {e.StatusCode}] - {e.ReasonPhrase}");
                return HealthCheckResult.Unhealthy(HealthCheckResultDescription, e);
            }
        }
    }
}
