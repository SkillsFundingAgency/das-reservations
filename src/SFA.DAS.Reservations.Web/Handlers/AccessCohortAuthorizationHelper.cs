﻿using System;
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
using SFA.DAS.Reservations.Domain.Providers.Api;
using SFA.DAS.Reservations.Infrastructure.Services;
using SFA.DAS.Reservations.Web.Infrastructure;

namespace SFA.DAS.Reservations.Web.Handlers;

public interface IAccessCohortAuthorizationHelper
{
    Task<bool> IsAuthorised();
}

public class AccessCohortAuthorizationHelper(
    ILogger<AccessCohortAuthorizationHelper> logger,
    IHttpContextAccessor httpContextAccessor,
    IEncodingService encodingService,
    IReservationsOuterService outerService) : IAccessCohortAuthorizationHelper
{
    public async Task<bool> IsAuthorised()
    {
        logger.LogInformation("AccessCohortAuthorizationHelper.IsAuthorised() claims: {claims}",
            JsonConvert.SerializeObject(httpContextAccessor.HttpContext.User.Claims.ToDictionary(claim => claim.Type, claim => claim.Value))
        );

        var hasEmployerAccountClaim = httpContextAccessor.HttpContext.User.HasClaim(x => x.Type.Equals(EmployerClaims.AccountsClaimsTypeIdentifier));

        // TODO Test this
        if (hasEmployerAccountClaim)
        {
            // Allow all employers through
            return true;
        }

        if (!httpContextAccessor.HttpContext.Request.RouteValues.TryGetValue(RouteValueKeys.AccountLegalEntityPublicHashedId, out var accountLegalEntityPublicHashedIdFromUrl))
        {
            logger.LogInformation("AccessCohortAuthorizationHelper.IsAuthorised() AccountLegalEntityPublicHashedId value was not found on the route.");
            return false;
        }

        var accountLegalEntityPublicHashedId = accountLegalEntityPublicHashedIdFromUrl?.ToString();
        if (string.IsNullOrEmpty(accountLegalEntityPublicHashedId))
        {
            logger.LogInformation("AccessCohortAuthorizationHelper.IsAuthorised() accountLegalEntityPublicHashedId value null or empty.");
            return false;
        }

        logger.LogInformation("AccessCohortAuthorizationHelper.IsAuthorised() accountLegalEntityPublicHashedId: {AccountLegalEntityPublicHashedId}.", accountLegalEntityPublicHashedId);

        var trustedAccountClaim = httpContextAccessor.HttpContext.User.GetClaimValue(ProviderClaims.AssociatedAccountsClaimsTypeIdentifier);

        Dictionary<long, AccountProviderLegalEntityDto> trustedAccounts;

        if (string.IsNullOrEmpty(trustedAccountClaim))
        {
            logger.LogInformation("AccessCohortAuthorizationHelper.IsAuthorised() no trusted account claims found. Retrieving from outerApi.");

            var providerIdClaim = httpContextAccessor.HttpContext.User.GetClaimValue(ProviderClaims.ProviderUkprn);

            logger.LogInformation("AccessCohortAuthorizationHelper.IsAuthorised() ProviderIdClaim value: {Id}.", providerIdClaim);

            if (!int.TryParse(providerIdClaim, out var providerId))
            {
                throw new ApplicationException($"Unable to parse providerId from ukprn claim value: {providerIdClaim}.");
            }

            // var legalEntitiesWithPermissionResponse = await outerService.GetAccountProviderLegalEntitiesWithCreateCohort(providerId);
            //
            // logger.LogInformation("AccessCohortAuthorizationHelper.IsAuthorised() response from APIM: {response}.", JsonConvert.SerializeObject(legalEntitiesWithPermissionResponse));

            // trustedAccounts = legalEntitiesWithPermissionResponse.AccountProviderLegalEntities.ToDictionary(x => x.AccountId);
            //
            // var trustedEmployersAsJson = JsonConvert.SerializeObject(trustedAccounts);
            //
            // var claimsIdentity = httpContextAccessor.HttpContext.User.Identities.First();
            //
            // claimsIdentity.AddClaim(new Claim(ProviderClaims.AssociatedAccountsClaimsTypeIdentifier, trustedEmployersAsJson, JsonClaimValueTypes.Json));
        }
        // else
        // {
        //     logger.LogInformation("AccessCohortAuthorizationHelper.IsAuthorised() trusted account claims found: {Claims}.", trustedAccountClaim);
        //
        //     try
        //     {
        //         trustedAccounts = JsonConvert.DeserializeObject<Dictionary<long, GetAccountProviderLegalEntitiesWithCreateCohortResponse.AccountProviderLegalEntityDto>>(trustedAccountClaim);
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
        // return trustedAccounts.ContainsKey(accountLegalEntityId);

        return false;
    }
}