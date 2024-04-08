using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
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

public class CommitmentsAuthorisationHandler(ICachedReservationsOuterService cachedOuterApiService, IHttpContextAccessor httpContextAccessor, IEncodingService encodingService) : ICommitmentsAuthorisationHandler
{
    public Task<bool> CanAccessCohort()
    {
        var providerId = GetProviderId();

        var cohortId = GetAndDecodeValueIfExists(RouteValueKeys.CohortReference, EncodingType.CohortReference);

        return cachedOuterApiService.CanAccessCohort(providerId, cohortId);
    }

    private int GetProviderId()
    {
        var providerIdClaim = httpContextAccessor.HttpContext.User.GetClaimValue(ProviderClaims.ProviderUkprn);

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