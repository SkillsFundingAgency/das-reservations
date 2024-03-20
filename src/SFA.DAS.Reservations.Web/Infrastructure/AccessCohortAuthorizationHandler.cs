using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SFA.DAS.DfESignIn.Auth.Extensions;
using SFA.DAS.Encoding;
using SFA.DAS.ProviderRelationships.Types.Models;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Domain.Providers.Api;
using SFA.DAS.Reservations.Domain.Reservations.Api;

namespace SFA.DAS.Reservations.Web.Infrastructure;

public class AccessCohortAuthorizationHandler(
    IHttpContextAccessor httpContextAccessor,
    ILogger<AccessCohortAuthorizationHandler> logger,
    IEncodingService encodingService,
    IReservationsOuterService outerService)
    : AuthorizationHandler<AccessCohortRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, AccessCohortRequirement requirement)
    {
        if (!httpContextAccessor.HttpContext.Request.RouteValues.ContainsKey(RouteValueKeys.AccountLegalEntityPublicHashedId))
        {
            return;
        }

        var providerIdClaim = context.User.GetClaimValue(ClaimsIdentity.DefaultNameClaimType);
        var accountLegalEntityPublicHashedIdFromUrl = httpContextAccessor.HttpContext.Request.RouteValues[RouteValueKeys.AccountLegalEntityPublicHashedId].ToString().ToUpper();

        if (string.IsNullOrEmpty(accountLegalEntityPublicHashedIdFromUrl))
        {
            return;
        }

        var trustedAccountClaim = context.User.GetClaimValue(ProviderClaims.TrustedEmployerAccounts);

        Dictionary<long, GetAccountProviderLegalEntitiesResponse.AccountProviderLegalEntityDto> trustedEmployers;
        
        if (trustedAccountClaim == null)
        {
            var providerId = int.Parse(providerIdClaim);
            var legalEntitiesWithPermissionResponse = await outerService.GetAccountProviderLegalEntities(providerId);
            
            trustedEmployers = legalEntitiesWithPermissionResponse.AccountProviderLegalEntities.ToDictionary(x => x.AccountId);
            
            var trustedEmployersAsJson = JsonConvert.SerializeObject(trustedEmployers);
            var claimsIdentity = (ClaimsIdentity)context.User.Identity;
            
            claimsIdentity.AddClaim(new Claim(ProviderClaims.TrustedEmployerAccounts, trustedEmployersAsJson, JsonClaimValueTypes.Json));
        }
        else
        {
            try
            {
                trustedEmployers = JsonConvert.DeserializeObject<Dictionary<long, GetAccountProviderLegalEntitiesResponse.AccountProviderLegalEntityDto>>(trustedAccountClaim);
            }
            catch (JsonSerializationException exception)
            {
                logger.LogError(exception, "Could not deserialize trusted accounts claim for provider.");
                return;
            }
        }

        var accountLegalEntityId = encodingService.Decode(accountLegalEntityPublicHashedIdFromUrl, EncodingType.AccountLegalEntityId);

        if (!trustedEmployers.ContainsKey(accountLegalEntityId))
        {
            return;
        }

        context.Succeed(requirement);
    }
}