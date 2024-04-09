using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SFA.DAS.DfESignIn.Auth.Extensions;
using SFA.DAS.Encoding;
using SFA.DAS.Reservations.Infrastructure.Services;
using SFA.DAS.Reservations.Web.Extensions;

namespace SFA.DAS.Reservations.Web.Infrastructure.Authorization.Helpers;

public interface IAccessCohortAuthorizationHelper
{
    Task<bool> CanAccessCohort();
}

public class AccessCohortAuthorizationHelper(
    ICachedReservationsOuterService cachedOuterApiService,
    IHttpContextAccessor httpContextAccessor,
    ILogger<AccessCohortAuthorizationHelper> logger,
    IEncodingService encodingService) : IAccessCohortAuthorizationHelper
{
    public async Task<bool> CanAccessCohort()
    {
        var user = httpContextAccessor.HttpContext?.User;
 
        // If the user is redirected to a controller action from another site (very likely) and this is method is executed, the claims will be empty until the middleware has
        // re-authenticated the user. Once authentication is confirmed this method will be executed again with the claims populated and will run properly.
        if (user.ClaimsAreEmpty())
        {
            logger.LogInformation("{TypeName} User Claims are empty.", nameof(AccessCohortAuthorizationHelper));
            return false;
        }

        if (user.IsEmployer())
        {
            return true;
        }

        var cohortId = GetAndDecodeValueIfExists(RouteValueKeys.CohortReference, EncodingType.CohortReference);

        logger.LogInformation("{TypeName} CohortId: {Id}.", nameof(AccessCohortAuthorizationHelper), cohortId);
        
        var providerIdClaim =  user.GetClaimValue(ProviderClaims.ProviderUkprn);

        if (!long.TryParse(providerIdClaim, out var providerId))
        {
            throw new ApplicationException($"{nameof(AccessCohortAuthorizationHelper)} Unable to parse providerId from ukprn claim value: {providerIdClaim}.");
        }

        logger.LogInformation("{TypeName} ProviderId: {Id}.", nameof(AccessCohortAuthorizationHelper), providerId);

        return await cachedOuterApiService.CanAccessCohort(providerId, cohortId);
    }

    private long GetAndDecodeValueIfExists(string keyName, EncodingType encodedType)
    {
        if (!httpContextAccessor.HttpContext.TryGetValueFromHttpContext(keyName, out var encodedValue))
        {
            return 0;
        }

        return encodingService.Decode(encodedValue, encodedType);
    }
}