using System;
using System.Linq;
using System.Security.Claims;
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
    // ICachedReservationsOuterService cachedOuterApiService, 
    IHttpContextAccessor httpContextAccessor,
    ILogger<AccessCohortAuthorizationHelper> logger,
    IEncodingService encodingService) : IAccessCohortAuthorizationHelper
{
    public async Task<bool> CanAccessCohort()
    {
        var user = httpContextAccessor.HttpContext?.User;

        logger.LogInformation("{TypeName} User Claims: {Claims}.", nameof(AccessCohortAuthorizationHelper),
            user.Claims.ToDictionary(x => x.Type, y => y.Value)
        );

        // if (user.IsEmployer())
        // {
        //     return true;
        // }

        var cohortId = GetAndDecodeValueIfExists(RouteValueKeys.CohortReference, EncodingType.CohortReference);

        logger.LogInformation("{TypeName} CohortId: {Id}.", nameof(AccessCohortAuthorizationHelper), cohortId);

        return false;

        //     var providerId = GetProviderId(user);
        //     
        //     return await cachedOuterApiService.CanAccessCohort(providerId, cohortId);
    }

    //
    // private static int GetProviderId(ClaimsPrincipal user)
    // {
    //     var providerIdClaim = user.GetClaimValue(ProviderClaims.ProviderUkprn);
    //
    //     if (!int.TryParse(providerIdClaim, out var providerId))
    //     {
    //         throw new ApplicationException($"{nameof(AccessCohortAuthorizationHelper)} Unable to parse providerId from ukprn claim value: {providerIdClaim}.");
    //     }
    //
    //     return providerId;
    // }
    
    private long GetAndDecodeValueIfExists(string keyName, EncodingType encodedType)
    {
        if (!httpContextAccessor.HttpContext.TryGetValueFromHttpContext(keyName, out var encodedValue))
        {
            return 0;
        }

        if (!encodingService.TryDecode(encodedValue, encodedType, out var id))
        {
            throw new UnauthorizedAccessException($"Failed to decode '{keyName}' value '{encodedValue}' using encoding type '{encodedType}'");
        }

        return id;
    }
}