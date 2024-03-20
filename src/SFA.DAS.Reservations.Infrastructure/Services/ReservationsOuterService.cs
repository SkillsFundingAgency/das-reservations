using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Domain.Providers.Api;
using SFA.DAS.Reservations.Domain.Reservations;
using SFA.DAS.Reservations.Domain.Reservations.Api;
using SFA.DAS.Reservations.Infrastructure.Api;
using SFA.DAS.Reservations.Infrastructure.Configuration;

namespace SFA.DAS.Reservations.Infrastructure.Services;

public class ReservationsOuterService(IReservationsOuterApiClient apiClient, IOptions<ReservationsOuterApiConfiguration> options)
    : IReservationsOuterService
{
    private readonly ReservationsOuterApiConfiguration _config = options.Value;

    public async Task<GetTransferValidityResponse> GetTransferValidity(long senderId, long receiverId, int? pledgeApplicationId = null)
    {
        var request = new GetTransferValidityRequest(_config.ApiBaseUrl, senderId, receiverId, pledgeApplicationId);

        return await apiClient.Get<GetTransferValidityResponse>(request);
    }

    public async Task<ProviderAccountResponse> GetProviderStatus(long ukprn)
    {
        return await apiClient.Get<ProviderAccountResponse>(new GetProviderStatusDetails(_config.ApiBaseUrl, ukprn));
    }
    
    public async Task<GetAccountProviderLegalEntitiesWithCreateCohortResponse> GetAccountProviderLegalEntitiesWithCreateCohort(long ukprn)
    {
        var request = new GetAccountProviderLegalEntitiesWithCreateCohortRequest(_config.ApiBaseUrl, ukprn);

        return await apiClient.Get<GetAccountProviderLegalEntitiesWithCreateCohortResponse>(request);
    }
}