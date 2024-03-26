using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.JsonWebTokens;
using Newtonsoft.Json;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Web.Infrastructure;

namespace SFA.DAS.Reservations.Web.Handlers;

public class ProviderAccountPostAuthenticationClaimsHandler(IReservationsOuterService outerService)
{
    public async Task<IEnumerable<Claim>> GetClaims(HttpContext httpContext, ClaimsPrincipal principal)
    {
        var providerIdValue = principal.Claims.First(c => c.Type.Equals(ProviderClaims.ProviderUkprn)).Value;
        var displayName = principal.Claims.First(c => c.Type.Equals(ProviderClaims.DisplayName)).Value;

        httpContext.Items.Add(ClaimsIdentity.DefaultNameClaimType, providerIdValue);
        httpContext.Items.Add(ProviderClaims.DisplayName, displayName);

        var claims = new List<Claim>
        {
            new(ClaimsIdentity.DefaultNameClaimType, providerIdValue),
            new(ProviderClaims.DisplayName, displayName)
        };

        var providerId = int.Parse(providerIdValue);

        var legalEntitiesWithPermissionResponse = await outerService.GetAccountProviderLegalEntitiesWithCreateCohort(providerId);

        var loggingService = httpContext.RequestServices.GetRequiredService(typeof(ILogger<ProviderAccountPostAuthenticationClaimsHandler>));

        if (loggingService is ILogger<ProviderAccountPostAuthenticationClaimsHandler> logger)
        {
            logger.LogWarning("ProviderAccountPostAuthenticationClaimsHandler.GetClaims() legalEntitiesWithPermissionResponse: {Response}", JsonConvert.SerializeObject(legalEntitiesWithPermissionResponse));
        }

        var trustedEmployers = legalEntitiesWithPermissionResponse.AccountProviderLegalEntities.ToDictionary(x => x.AccountId);
        var trustedEmployersAsJson = JsonConvert.SerializeObject(trustedEmployers);

        claims.Add(new Claim(ProviderClaims.TrustedEmployerAccounts, trustedEmployersAsJson, JsonClaimValueTypes.Json));

        principal.Identities.First().AddClaims(claims);

        return claims;
    }
}