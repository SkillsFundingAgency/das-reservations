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
using SFA.DAS.Reservations.Domain.Providers.Api;
using SFA.DAS.Reservations.Web.Extensions;
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
        var user = httpContextAccessor.HttpContext?.User;

        if (ClaimsAreEmptyFor(user))
        {
            logger.LogInformation("{TypeName} User Claims are empty.", nameof(AccessCohortAuthorizationHelper));
            return false;
        }

        if (user.IsEmployer())
        {
            return true;
        }

        if (!TryGetAccountLegalEntityPublicHashedId(out var accountLegalEntityPublicHashedId))
        {
            logger.LogInformation("{TypeName} AccountLegalEntityPublicHashedId value was not found on the route.", nameof(AccessCohortAuthorizationHelper));
            return false;
        }

        var trustedAccountClaim = user.GetClaimValue(ProviderClaims.TrustedAccounts);

        List<GetAccountLegalEntitiesForProviderItem> trustedAccounts;

        if (string.IsNullOrEmpty(trustedAccountClaim))
        {
            trustedAccounts = await GetAccountLegalEntitiesFromOuterApi(user);

            AddTrustedAccountsToClaims(user, trustedAccounts);
        }
        else
        {
            trustedAccounts = GetAccountLegalEntitiesFromClaims(trustedAccountClaim);

            if (trustedAccounts == null)
            {
                return false;
            }
        }

        var accountLegalEntityId = encodingService.Decode(accountLegalEntityPublicHashedId?.ToString(), EncodingType.PublicAccountLegalEntityId);

        logger.LogInformation("{TypeName} accountLegalEntityId from Route: {accountLegalEntityId}.", nameof(AccessCohortAuthorizationHelper), accountLegalEntityId);

        var accountLegalEntityIdFound = trustedAccounts.Exists(x => x.AccountLegalEntityId == accountLegalEntityId);

        logger.LogInformation("{TypeName} accountLegalEntityIdFound: {accountLegalEntityIdFound}.", nameof(AccessCohortAuthorizationHelper), accountLegalEntityIdFound);

        return accountLegalEntityIdFound;
    }

    private static bool ClaimsAreEmptyFor(ClaimsPrincipal user)
    {
        return !user.Claims.Any();
    }

    private static void AddTrustedAccountsToClaims(ClaimsPrincipal user, List<GetAccountLegalEntitiesForProviderItem> trustedAccounts)
    {
        user.Identities
            .First()
            .AddClaim(new Claim(ProviderClaims.TrustedAccounts, JsonConvert.SerializeObject(trustedAccounts), JsonClaimValueTypes.Json));
    }

    private bool TryGetAccountLegalEntityPublicHashedId(out object accountLegalEntityPublicHashedId)
    {
        return httpContextAccessor.HttpContext.Request.RouteValues.TryGetValue(RouteValueKeys.AccountLegalEntityPublicHashedId, out accountLegalEntityPublicHashedId);
    }

    private async Task<List<GetAccountLegalEntitiesForProviderItem>> GetAccountLegalEntitiesFromOuterApi(ClaimsPrincipal user)
    {
        logger.LogInformation("{TypeName} no trusted account claims found. Retrieving from outerApi.", nameof(AccessCohortAuthorizationHelper));

        var providerIdClaim = user.GetClaimValue(ProviderClaims.ProviderUkprn);

        if (!int.TryParse(providerIdClaim, out var providerId))
        {
            throw new ApplicationException($"{nameof(AccessCohortAuthorizationHelper)} Unable to parse providerId from ukprn claim value: {providerIdClaim}.");
        }

        var response = await outerService.GetAccountProviderLegalEntitiesWithCreateCohort(providerId);

        return response.AccountProviderLegalEntities;
    }

    private List<GetAccountLegalEntitiesForProviderItem> GetAccountLegalEntitiesFromClaims(string trustedAccountClaim)
    {
        List<GetAccountLegalEntitiesForProviderItem> trustedAccounts;
        logger.LogInformation("{TypeName} trusted account claims found.", nameof(AccessCohortAuthorizationHelper));

        try
        {
            trustedAccounts = JsonConvert.DeserializeObject<List<GetAccountLegalEntitiesForProviderItem>>(trustedAccountClaim);
            logger.LogInformation("{TypeName} trusted account claims: {Claims}.", nameof(AccessCohortAuthorizationHelper), JsonConvert.SerializeObject(trustedAccounts));
        }
        catch (JsonSerializationException exception)
        {
            logger.LogError(exception, "{TypeName} Could not deserialize trusted accounts claim for provider.", nameof(AccessCohortAuthorizationHelper));
            return null;
        }

        return trustedAccounts;
    }
}