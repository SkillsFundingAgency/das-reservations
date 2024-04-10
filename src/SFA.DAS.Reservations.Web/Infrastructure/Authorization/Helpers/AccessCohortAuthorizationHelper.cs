using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.JsonWebTokens;
using Newtonsoft.Json;
using SFA.DAS.DfESignIn.Auth.Extensions;
using SFA.DAS.Encoding;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Web.Extensions;

namespace SFA.DAS.Reservations.Web.Infrastructure.Authorization.Helpers;

public interface IAccessCohortAuthorizationHelper
{
    Task<bool> CanAccessCohort();
}

public class AccessCohortAuthorizationHelper(
    IReservationsOuterService outerApiService,
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
            logger.LogWarning("{TypeName} User Claims are empty.", nameof(AccessCohortAuthorizationHelper));
            return false;
        }

        if (user.IsEmployer())
        {
            return true;
        }

        var cohortId = GetAndDecodeValueIfExists(RouteValueKeys.CohortReference, EncodingType.CohortReference);

        var providerIdClaim = user.GetClaimValue(ProviderClaims.ProviderUkprn);

        if (!long.TryParse(providerIdClaim, out var providerId))
        {
            throw new ApplicationException($"{nameof(AccessCohortAuthorizationHelper)} Unable to parse providerId from ukprn claim value: {providerIdClaim}.");
        }

        var accessibleCohorts = JsonConvert.DeserializeObject<Dictionary<long, bool>>(user.GetClaimValue(ProviderClaims.AccessibleCohorts));

        if (AccessibleCohortExistsOnClaims(accessibleCohorts, cohortId))
        {
            return accessibleCohorts[cohortId];
        }

        var canAccessCohort = await outerApiService.CanAccessCohort(providerId, cohortId);

        AddResultToAccessibleCohorts(accessibleCohorts, cohortId, canAccessCohort);

        if (!user.HasClaim(x => x.Type.Equals(ProviderClaims.AccessibleCohorts)))
        {
            AddAccessibleCohortsClaim(user, cohortId, canAccessCohort);
        }
        else
        {
            UpdateAccessibleCohortsClaim(user, accessibleCohorts);
        }

        return canAccessCohort;
    }

    private static void AddResultToAccessibleCohorts(Dictionary<long, bool> accessibleCohorts, long cohortId, bool canAccessCohort)
    {
        if (accessibleCohorts == null)
        {
            accessibleCohorts = new Dictionary<long, bool>();
        }

        accessibleCohorts.Add(cohortId, canAccessCohort);
    }

    private static bool AccessibleCohortExistsOnClaims(IReadOnlyDictionary<long, bool> accessibleCohorts, long cohortId)
    {
        return accessibleCohorts != null && accessibleCohorts.Any() && accessibleCohorts.ContainsKey(cohortId);
    }

    private static void AddAccessibleCohortsClaim(ClaimsPrincipal user, long cohortId, bool canAccessCohort)
    {
        user.Identities
            .First()
            .AddClaim(new Claim(ProviderClaims.AccessibleCohorts, JsonConvert.SerializeObject(new Dictionary<long, bool> { { cohortId, canAccessCohort } }), JsonClaimValueTypes.Json));
    }

    private static void UpdateAccessibleCohortsClaim(ClaimsPrincipal user, IReadOnlyDictionary<long, bool> accessibleCohorts)
    {
        var claimsIdentity = user.Identities.First();
        
        claimsIdentity.RemoveClaim(claimsIdentity.FindFirst(x => x.Type.Equals(ProviderClaims.AccessibleCohorts)));

        claimsIdentity.AddClaim(new Claim(ProviderClaims.AccessibleCohorts, JsonConvert.SerializeObject(accessibleCohorts), JsonClaimValueTypes.Json));
    }

    private long GetAndDecodeValueIfExists(string keyName, EncodingType encodedType)
    {
        return !httpContextAccessor.HttpContext.TryGetValueFromHttpContext(keyName, out var encodedValue) 
            ? 0 
            : encodingService.Decode(encodedValue, encodedType);
    }
}