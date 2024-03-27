using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SFA.DAS.Encoding;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Domain.Providers.Api;

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
        logger.LogWarning("AccessCohortAuthorizationHandler.HandleRequirementAsync() starting.");

        if (!await IsProviderAuthorised(context))
        {
            return;
        }

        context.Succeed(requirement);
    }

    public async Task<bool> IsProviderAuthorised(AuthorizationHandlerContext context)
    {
        if (!httpContextAccessor.HttpContext.Request.RouteValues.TryGetValue(RouteValueKeys.AccountLegalEntityPublicHashedId, out var accountLegalEntityPublicHashedIdFromUrl))
        {
            logger.LogInformation("AccessCohortAuthorizationHandler.IsProviderAuthorised() AccountLegalEntityPublicHashedId value was not found on the route.");
            return false;
        }

        var accountLegalEntityPublicHashedId = accountLegalEntityPublicHashedIdFromUrl?.ToString();
        if (string.IsNullOrEmpty(accountLegalEntityPublicHashedId))
        {
            return false;
        }

        var claimsIdentity = (ClaimsIdentity)context.User.Identity;

        var trustedAccountClaim = claimsIdentity.Claims.FirstOrDefault(c => c.Type.Equals(ProviderClaims.TrustedEmployerAccounts))?.Value;

        logger.LogInformation("AccessCohortAuthorizationHandler.IsProviderAuthorised() claims: {claims}",
            JsonConvert.SerializeObject(claimsIdentity.Claims.ToDictionary(claim => claim.Type, claim => claim.Value))
        );

        Dictionary<long, GetAccountProviderLegalEntitiesWithCreateCohortResponse.AccountProviderLegalEntityDto> trustedEmployers;

        if (trustedAccountClaim == null || string.IsNullOrEmpty(trustedAccountClaim))
        {
            logger.LogInformation("AccessCohortAuthorizationHandler.IsProviderAuthorised() no trusted account claims found. Retrieving from outerApi.");

            var providerIdClaim = claimsIdentity.Claims.FirstOrDefault(claim => claim.Type == ProviderClaims.ProviderUkprn)?.Value;

            logger.LogInformation("AccessCohortAuthorizationHandler.IsProviderAuthorised() ProviderIdClaim value: {Id}.", providerIdClaim);

            if (!int.TryParse(providerIdClaim, out var providerId))
            {
                throw new ApplicationException($"Unable to parse providerId from ukprn claim value: {providerIdClaim}.");
            }

            var legalEntitiesWithPermissionResponse = await outerService.GetAccountProviderLegalEntitiesWithCreateCohort(providerId);

            logger.LogInformation("AccessCohortAuthorizationHandler.IsProviderAuthorised() response from APIM: {response}.", JsonConvert.SerializeObject(legalEntitiesWithPermissionResponse));

            trustedEmployers = legalEntitiesWithPermissionResponse.AccountProviderLegalEntities.ToDictionary(x => x.AccountId);

            var trustedEmployersAsJson = JsonConvert.SerializeObject(trustedEmployers);

            claimsIdentity.AddClaim(new Claim(ProviderClaims.TrustedEmployerAccounts, trustedEmployersAsJson, JsonClaimValueTypes.Json));
        }
        else
        {
            logger.LogInformation("AccessCohortAuthorizationHandler.IsProviderAuthorised() trusted account claims found: {Claims}.", trustedAccountClaim);

            try
            {
                trustedEmployers = JsonConvert.DeserializeObject<Dictionary<long, GetAccountProviderLegalEntitiesWithCreateCohortResponse.AccountProviderLegalEntityDto>>(trustedAccountClaim);
            }
            catch (JsonSerializationException exception)
            {
                logger.LogError(exception, "Could not deserialize trusted accounts claim for provider.");
                return false;
            }
        }

        var accountLegalEntityId = encodingService.Decode(accountLegalEntityPublicHashedId, EncodingType.AccountLegalEntityId);

        return trustedEmployers.ContainsKey(accountLegalEntityId);
    }
}