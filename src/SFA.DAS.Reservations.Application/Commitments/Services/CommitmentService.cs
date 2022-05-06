using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using SFA.DAS.Reservations.Domain.Commitments;
using SFA.DAS.Reservations.Domain.Commitments.Api;
using SFA.DAS.Reservations.Infrastructure.Api;
using SFA.DAS.Reservations.Infrastructure.Configuration;

namespace SFA.DAS.Reservations.Application.Commitments.Services
{
    public class CommitmentService : ICommitmentService
    {
        private readonly IReservationsOuterApiClient _apiClient;
        private readonly ReservationsOuterApiConfiguration _config;

        public CommitmentService(IReservationsOuterApiClient apiClient, IOptions<ReservationsOuterApiConfiguration> options)
        {
            _apiClient = apiClient;
            _config = options.Value;
        }

        public async Task<Cohort> GetCohort(long cohortId)
        {
            var request = new GetCohortRequest(_config.ApiBaseUrl, cohortId);
            var cohort = await _apiClient.Get<Cohort>(request);

            return cohort;
        }
    }
}
