﻿using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using SFA.DAS.Http;
using SFA.DAS.Reservations.Infrastructure.Api;
using SFA.DAS.Reservations.Infrastructure.Extensions;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.Reservations.Infrastructure.HealthCheck
{
    public class ReservationsApiHealthCheck : IHealthCheck
    {
        private const string HealthCheckResultDescription = "Reservation Api check";
        
        private readonly IApiClient _apiClient;
        private readonly ILogger<ReservationsApiHealthCheck> _logger;

        public ReservationsApiHealthCheck(IApiClient apiClient, ILogger<ReservationsApiHealthCheck> logger)
        {
            _apiClient = apiClient;
            _logger = logger;
        }
        
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Pinging Reservation API");
            
            try
            {
                var timer = Stopwatch.StartNew();
                await _apiClient.Ping();
                timer.Stop();

                var durationString = timer.Elapsed.ToHumanReadableString();
                
                _logger.LogInformation($"Reservation API ping successful and took {durationString}");
                
                return HealthCheckResult.Healthy(HealthCheckResultDescription, 
                    new Dictionary<string, object>{{"Duration", durationString}});
            }
            catch (RestHttpClientException e)
            {
                _logger.LogWarning($"Reservation API ping failed : [Code: {e.StatusCode}] - {e.ReasonPhrase}");
                return HealthCheckResult.Unhealthy(HealthCheckResultDescription, e);
            }
        }
    }
}
