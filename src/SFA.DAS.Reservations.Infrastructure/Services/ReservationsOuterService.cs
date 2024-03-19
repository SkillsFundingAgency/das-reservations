using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Domain.Providers.Api;
using SFA.DAS.Reservations.Domain.Reservations;
using SFA.DAS.Reservations.Domain.Reservations.Api;
using SFA.DAS.Reservations.Infrastructure.Api;
using SFA.DAS.Reservations.Infrastructure.Configuration;

namespace SFA.DAS.Reservations.Infrastructure.Services;

public class ReservationsOuterService : IReservationsOuterService
{
    private readonly IReservationsOuterApiClient _apiClient;
    private readonly ReservationsOuterApiConfiguration _config;

    public ReservationsOuterService(IReservationsOuterApiClient apiClient, IOptions<ReservationsOuterApiConfiguration> options)
    {
        _apiClient = apiClient;
        _config = options.Value;
    }

    public async Task<GetTransferValidityResponse> GetTransferValidity(long senderId, long receiverId, int? pledgeApplicationId = null)
    {
        var request = new GetTransferValidityRequest(_config.ApiBaseUrl, senderId, receiverId, pledgeApplicationId);

        return await _apiClient.Get<GetTransferValidityResponse>(request);
    }

    public async Task<ProviderAccountResponse> GetProviderStatus(long ukprn)
    {
        return await _apiClient.Get<ProviderAccountResponse>(new GetProviderStatusDetails(_config.ApiBaseUrl, ukprn));
    }

    public async Task<bool> CanAccessCohort(Party party, long partyId, long cohortId)
    {
        var content = new GetCohortAccessRequest(_config.ApiBaseUrl, party, partyId, cohortId);

        var response = await _apiClient.Get<GetCohortAccessResponse>(content);

        return response.HasCohortAccess;
    }
}