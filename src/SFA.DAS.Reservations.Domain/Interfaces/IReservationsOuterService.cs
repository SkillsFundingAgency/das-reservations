using System.Threading.Tasks;
using SFA.DAS.Reservations.Domain.Providers.Api;
using SFA.DAS.Reservations.Domain.Reservations;

namespace SFA.DAS.Reservations.Domain.Interfaces;

public interface IReservationsOuterService
{
    Task<GetTransferValidityResponse> GetTransferValidity(long senderId, long receiverId, int? pledgeApplicationId = null);
    Task<ProviderAccountResponse> GetProviderStatus(long ukprn);
    Task<GetAccountLegalEntitiesForProviderResponse> GetAccountProviderLegalEntitiesWithCreateCohort(long ukprn);
}