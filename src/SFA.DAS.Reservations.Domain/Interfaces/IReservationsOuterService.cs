using System.Collections.Generic;
using System.Threading.Tasks;
using SFA.DAS.Reservations.Domain.Employers;
using SFA.DAS.Reservations.Domain.Providers.Api;
using SFA.DAS.Reservations.Domain.Reservations;
using SFA.DAS.Reservations.Domain.Rules.Api;

namespace SFA.DAS.Reservations.Domain.Interfaces;

public interface IReservationsOuterService
{
    Task<GetTransferValidityResponse> GetTransferValidity(long senderId, long receiverId, int? pledgeApplicationId = null);
    Task<ProviderAccountResponse> GetProviderStatus(long ukprn);
    Task<GetAccountLegalEntitiesForProviderResponse> GetAccountProviderLegalEntitiesWithCreateCohort(long ukprn);
    Task<bool> CanAccessCohort(long partyId, long cohortId);
    Task<GetAvailableDatesApiResponse> GetAvailableDates(long accountLegalEntityId);
    Task<IEnumerable<Employer>> GetTrustedEmployers(uint ukPrn);
}