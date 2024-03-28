using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SFA.DAS.Reservations.Web.Infrastructure;

namespace SFA.DAS.Reservations.Web.Handlers;

public interface IAccessCohortAuthorizationHelper
{
    bool IsAuthorised(AuthorizationHandlerContext context);
}

public class AccessCohortAuthorizationHelper(ILogger<AccessCohortAuthorizationHelper> logger, IHttpContextAccessor httpContextAccessor) : IAccessCohortAuthorizationHelper
{
    public bool IsAuthorised(AuthorizationHandlerContext context)
    {
        logger.LogInformation("AccessCohortAuthorizationHelper.IsAuthorised() claims: {claims}",
            JsonConvert.SerializeObject(context.User.Claims.ToDictionary(claim => claim.Type, claim => claim.Value))
        );
        
        if (!httpContextAccessor.HttpContext.Request.RouteValues.TryGetValue(RouteValueKeys.AccountLegalEntityPublicHashedId, out var accountLegalEntityPublicHashedIdFromUrl))
        {
            logger.LogInformation("AccessCohortAuthorizationHelper.IsAuthorised() AccountLegalEntityPublicHashedId value was not found on the route.");
            return false;
        }
        
        var accountLegalEntityPublicHashedId = accountLegalEntityPublicHashedIdFromUrl?.ToString();
        if (string.IsNullOrEmpty(accountLegalEntityPublicHashedId))
        {
            return false;
        }
        
        var trustedAccountClaim = context.User.FindFirst(c => c.Type.Equals(ProviderClaims.TrustedEmployerAccounts))?.Value;

        return false;

        // Dictionary<long, GetAccountProviderLegalEntitiesWithCreateCohortResponse.AccountProviderLegalEntityDto> trustedEmployers;
        //
        // if (trustedAccountClaim == null || string.IsNullOrEmpty(trustedAccountClaim))
        // {
        //     logger.LogInformation("AccessCohortAuthorizationHandler.IsAuthorisedToAccessCohort() no trusted account claims found. Retrieving from outerApi.");
        //
        //     var providerIdClaim = context.User.FindFirst(c => c.Type.Equals(ProviderClaims.ProviderUkprn)).Value;
        //
        //     logger.LogInformation("AccessCohortAuthorizationHandler.IsAuthorisedToAccessCohort() ProviderIdClaim value: {Id}.", providerIdClaim);
        //
        //     if (!int.TryParse(providerIdClaim, out var providerId))
        //     {
        //         throw new ApplicationException($"Unable to parse providerId from ukprn claim value: {providerIdClaim}.");
        //     }
        //
        //     var legalEntitiesWithPermissionResponse = await outerService.GetAccountProviderLegalEntitiesWithCreateCohort(providerId);
        //
        //     logger.LogInformation("AccessCohortAuthorizationHandler.IsAuthorisedToAccessCohort() response from APIM: {response}.", JsonConvert.SerializeObject(legalEntitiesWithPermissionResponse));
        //
        //     trustedEmployers = legalEntitiesWithPermissionResponse.AccountProviderLegalEntities.ToDictionary(x => x.AccountId);
        //
        //     var trustedEmployersAsJson = JsonConvert.SerializeObject(trustedEmployers);
        //
        //     var claimsIdentity = context.User.Identities.First();
        //
        //     claimsIdentity.AddClaim(new Claim(ProviderClaims.TrustedEmployerAccounts, trustedEmployersAsJson, JsonClaimValueTypes.Json));
        // }
        // else
        // {
        //     logger.LogInformation("AccessCohortAuthorizationHandler.IsAuthorisedToAccessCohort() trusted account claims found: {Claims}.", trustedAccountClaim);
        //
        //     try
        //     {
        //         trustedEmployers = JsonConvert.DeserializeObject<Dictionary<long, GetAccountProviderLegalEntitiesWithCreateCohortResponse.AccountProviderLegalEntityDto>>(trustedAccountClaim);
        //     }
        //     catch (JsonSerializationException exception)
        //     {
        //         logger.LogError(exception, "Could not deserialize trusted accounts claim for provider.");
        //         return false;
        //     }
        // }
        //
        // var accountLegalEntityId = encodingService.Decode(accountLegalEntityPublicHashedId, EncodingType.AccountLegalEntityId);
        //
        // return trustedEmployers.ContainsKey(accountLegalEntityId);
    }
}