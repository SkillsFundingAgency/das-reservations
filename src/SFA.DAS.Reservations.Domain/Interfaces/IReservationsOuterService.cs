using System.Threading.Tasks;
using SFA.DAS.ProviderRelationships.Types.Models;
using SFA.DAS.Reservations.Domain.Providers.Api;
using SFA.DAS.Reservations.Domain.Reservations;
using SFA.DAS.Reservations.Domain.Reservations.Api;

namespace SFA.DAS.Reservations.Domain.Interfaces;

public interface IReservationsOuterService
{
    Task<GetTransferValidityResponse> GetTransferValidity(long senderId, long receiverId, int? pledgeApplicationId = null);
    Task<ProviderAccountResponse> GetProviderStatus(long ukprn);
    Task<GetAccountProviderLegalEntitiesWithPermissionResponse> GetAccountProviderLegalEntitiesWithPermission(long ukprn, Operation operation);
}