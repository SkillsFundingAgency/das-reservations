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
using SFA.DAS.Reservations.Web.Infrastructure;

namespace SFA.DAS.Reservations.Web.Handlers;

public interface ICommitmentsAuthorisationHandler
{
    Task<bool> CanAccessCohort();
}

public class CommitmentsAuthorisationHandler(
    ICachedReservationsOuterService cachedOuterApiService, 
    IHttpContextAccessor httpContextAccessor, 
    ILogger<CommitmentsAuthorisationHandler> logger,
    IEncodingService encodingService) : ICommitmentsAuthorisationHandler
{
    public Task<bool> CanAccessCohort()
    {
        var user = httpContextAccessor.HttpContext?.User;
        
        logger.LogInformation("{TypeName} User Claims: {Claims}.", nameof(CommitmentsAuthorisationHandler),
            user.Claims.ToDictionary(x => x.Type, y=> y.Value)
            );
        
        if (user.IsEmployer())
        {
            return Task.FromResult(true);
        }
        
        var cohortId = GetAndDecodeValueIfExists(RouteValueKeys.CohortReference, EncodingType.CohortReference);

        logger.LogInformation("{TypeName} CohortId: {Id}.", nameof(CommitmentsAuthorisationHandler), cohortId);
            
        var providerId = GetProviderId(user);
        
        return cachedOuterApiService.CanAccessCohort(providerId, cohortId);
    }

    private static int GetProviderId(ClaimsPrincipal user)
    {
        var providerIdClaim = user.GetClaimValue(ProviderClaims.ProviderUkprn);

        if (!int.TryParse(providerIdClaim, out var providerId))
        {
            throw new ApplicationException($"{nameof(CommitmentsAuthorisationHandler)} Unable to parse providerId from ukprn claim value: {providerIdClaim}.");
        }

        return providerId;
    }

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