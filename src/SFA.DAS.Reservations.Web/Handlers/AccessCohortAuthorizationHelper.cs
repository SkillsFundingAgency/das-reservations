using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.JsonWebTokens;
using Newtonsoft.Json;
using SFA.DAS.Encoding;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Domain.Providers.Api;
using SFA.DAS.Reservations.Infrastructure.Services;
using SFA.DAS.Reservations.Web.Infrastructure;

namespace SFA.DAS.Reservations.Web.Handlers;

public interface IAccessCohortAuthorizationHelper
{
    bool IsAuthorised();
}

public class AccessCohortAuthorizationHelper(
    ILogger<AccessCohortAuthorizationHelper> logger,
    IHttpContextAccessor httpContextAccessor,
    IEncodingService encodingService,
    IReservationsOuterService outerService) : IAccessCohortAuthorizationHelper
{
    public bool IsAuthorised()
    {
        logger.LogInformation("AccessCohortAuthorizationHelper.IsAuthorised() claims: {claims}",
            JsonConvert.SerializeObject(httpContextAccessor.HttpContext.User.Claims.ToDictionary(claim => claim.Type, claim => claim.Value))
        );

        var employerAccountClaim = httpContextAccessor.HttpContext.User.FindFirst(c => c.Type.Equals(EmployerClaims.AccountsClaimsTypeIdentifier));

        // TODO Test this
        if (employerAccountClaim?.Value != null)
        {
            // Allow all employers through
            return true;
        }

        return false;

        // if (!httpContextAccessor.HttpContext.Request.RouteValues.TryGetValue(RouteValueKeys.AccountLegalEntityPublicHashedId, out var accountLegalEntityPublicHashedIdFromUrl))
        // {
        //     logger.LogInformation("AccessCohortAuthorizationHelper.IsAuthorised() AccountLegalEntityPublicHashedId value was not found on the route.");
        //     return false;
        // }
        //
        // var accountLegalEntityPublicHashedId = accountLegalEntityPublicHashedIdFromUrl?.ToString();
        // if (string.IsNullOrEmpty(accountLegalEntityPublicHashedId))
        // {
        //     return false;
        // }
        //
        // var trustedAccountClaim = context.User.FindFirst(c => c.Type.Equals(ProviderClaims.TrustedEmployerAccounts))?.Value;
        //
        // Dictionary<long, GetAccountProviderLegalEntitiesWithCreateCohortResponse.AccountProviderLegalEntityDto> trustedEmployers;
        //
        // if (trustedAccountClaim == null || string.IsNullOrEmpty(trustedAccountClaim))
        // {
        //     logger.LogInformation("AccessCohortAuthorizationHelper.IsAuthorised() no trusted account claims found. Retrieving from outerApi.");
        //
        //     var providerIdClaim = context.User.FindFirst(c => c.Type.Equals(ProviderClaims.ProviderUkprn)).Value;
        //
        //     logger.LogInformation("AccessCohortAuthorizationHelper.IsAuthorised() ProviderIdClaim value: {Id}.", providerIdClaim);
        //
        //     if (!int.TryParse(providerIdClaim, out var providerId))
        //     {
        //         throw new ApplicationException($"Unable to parse providerId from ukprn claim value: {providerIdClaim}.");
        //     }
        //
        //     var legalEntitiesWithPermissionResponse = await outerService.GetAccountProviderLegalEntitiesWithCreateCohort(providerId);
        //
        //     logger.LogInformation("AccessCohortAuthorizationHelper.IsAuthorised() response from APIM: {response}.", JsonConvert.SerializeObject(legalEntitiesWithPermissionResponse));
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
        //     logger.LogInformation("AccessCohortAuthorizationHelper.IsAuthorised() trusted account claims found: {Claims}.", trustedAccountClaim);
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