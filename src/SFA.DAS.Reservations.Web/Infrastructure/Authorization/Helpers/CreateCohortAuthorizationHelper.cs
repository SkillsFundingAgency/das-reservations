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

namespace SFA.DAS.Reservations.Web.Infrastructure.Authorization.Helpers;

public interface ICreateCohortAuthorizationHelper
{
    Task<bool> CanCreateCohort();
}

public class CreateCohortAuthorizationHelper(
    ILogger<CreateCohortAuthorizationHelper> logger,
    IHttpContextAccessor httpContextAccessor,
    IEncodingService encodingService,
    IReservationsOuterService outerService) : ICreateCohortAuthorizationHelper
{
    public async Task<bool> CanCreateCohort()
    {
        var user = httpContextAccessor.HttpContext?.User;

        if (ClaimsAreEmptyFor(user))
        {
            logger.LogInformation("{TypeName} User Claims are empty.", nameof(CreateCohortAuthorizationHelper));
            return false;
        }

        if (user.IsEmployer())
        {
            return true;
        }

        if (!TryGetAccountLegalEntityPublicHashedId(out var accountLegalEntityPublicHashedId))
        {
            logger.LogInformation("{TypeName} AccountLegalEntityPublicHashedId value was not found on the route.", nameof(CreateCohortAuthorizationHelper));
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

        logger.LogInformation("{TypeName} accountLegalEntityId from Route: {accountLegalEntityId}.", nameof(CreateCohortAuthorizationHelper), accountLegalEntityId);

        var accountLegalEntityIdFound = trustedAccounts.Exists(x => x.AccountLegalEntityId == accountLegalEntityId);

        logger.LogInformation("{TypeName} accountLegalEntityIdFound: {accountLegalEntityIdFound}.", nameof(CreateCohortAuthorizationHelper), accountLegalEntityIdFound);

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
        logger.LogInformation("{TypeName} no trusted account claims found. Retrieving from outerApi.", nameof(CreateCohortAuthorizationHelper));

        var providerIdClaim = user.GetClaimValue(ProviderClaims.ProviderUkprn);

        if (!int.TryParse(providerIdClaim, out var providerId))
        {
            throw new ApplicationException($"{nameof(CreateCohortAuthorizationHelper)} Unable to parse providerId from ukprn claim value: {providerIdClaim}.");
        }

        var response = await outerService.GetAccountProviderLegalEntitiesWithCreateCohort(providerId);

        return response.AccountProviderLegalEntities;
    }

    private List<GetAccountLegalEntitiesForProviderItem> GetAccountLegalEntitiesFromClaims(string trustedAccountClaim)
    {
        List<GetAccountLegalEntitiesForProviderItem> trustedAccounts;
        logger.LogInformation("{TypeName} trusted account claims found.", nameof(CreateCohortAuthorizationHelper));

        try
        {
            trustedAccounts = JsonConvert.DeserializeObject<List<GetAccountLegalEntitiesForProviderItem>>(trustedAccountClaim);
            logger.LogInformation("{TypeName} trusted account claims: {Claims}.", nameof(CreateCohortAuthorizationHelper), JsonConvert.SerializeObject(trustedAccounts));
        }
        catch (JsonSerializationException exception)
        {
            logger.LogError(exception, "{TypeName} Could not deserialize trusted accounts claim for provider.", nameof(CreateCohortAuthorizationHelper));
            return null;
        }

        return trustedAccounts;
    }
}