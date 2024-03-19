using System.Collections.Generic;
using System.Threading.Tasks;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Reservations.Infrastructure.Services;

namespace SFA.DAS.Reservations.Web.Infrastructure;

public interface ICommitmentsAuthorisationHandler
{
    Task<bool> CanAccessCohort();
}

public class CommitmentsAuthorisationHandler(ICachedOuterApiService cachedOuterApiService, IAuthorizationValueProvider authorizationValueProvider) : ICommitmentsAuthorisationHandler
{
    public Task<bool> CanAccessCohort()
    {
        var permissionValues = GetPermissionValues();

        return cachedOuterApiService.CanAccessCohort(Party.Provider, permissionValues.PartyId, permissionValues.CohortId);
    }
    
    private (long CohortId, long ApprenticeshipId, long PartyId) GetPermissionValues()
    {
        var cohortId = authorizationValueProvider.GetCohortId();
        var apprenticeshipId = authorizationValueProvider.GetApprenticeshipId();
        var providerId = authorizationValueProvider.GetProviderId();
        
        
        if (cohortId == 0 && apprenticeshipId == 0 && providerId == 0)
        {
            throw new KeyNotFoundException("At least one key of 'ProviderId', 'CohortId' or 'ApprenticeshipId' should be present in the authorization context");
        }

        return (cohortId, apprenticeshipId, providerId);
    }
}