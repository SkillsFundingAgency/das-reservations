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
using SFA.DAS.Reservations.Application.Providers.Services;
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
        var user = httpContextAccessor.HttpContext?.User;

        if (!user.Claims.Any())
        {
            logger.LogInformation("{TypeName} User Claims are empty.", nameof(AccessCohortAuthorizationHelper));
            return false;
        }

        var isEmployerUser = user.HasClaim(x => x.Type.Equals(EmployerClaims.AccountsClaimsTypeIdentifier));

        if (isEmployerUser)
        {
            return true;
        }

        if (!httpContextAccessor.HttpContext.Request.RouteValues.TryGetValue(RouteValueKeys.AccountLegalEntityPublicHashedId, out var accountLegalEntityPublicHashedId))
        {
            logger.LogInformation("{TypeName} AccountLegalEntityPublicHashedId value was not found on the route.", nameof(AccessCohortAuthorizationHelper));
            return false;
        }

        var trustedAccountClaim = user.GetClaimValue(ProviderClaims.AssociatedAccountsClaimsTypeIdentifier);

        Dictionary<long, GetProviderAccountLegalEntityWithCreatCohortItem> trustedAccounts;

        if (string.IsNullOrEmpty(trustedAccountClaim))
        {
            logger.LogInformation("{TypeName} no trusted account claims found. Retrieving from outerApi.", nameof(AccessCohortAuthorizationHelper));

            var providerIdClaim = user.GetClaimValue(ProviderClaims.ProviderUkprn);

            if (!int.TryParse(providerIdClaim, out var providerId))
            {
                throw new ApplicationException($"{nameof(AccessCohortAuthorizationHelper)} Unable to parse providerId from ukprn claim value: {providerIdClaim}.");
            }

            var legalEntitiesWithPermissionResponse = await outerService.GetAccountProviderLegalEntitiesWithCreateCohort(providerId);

            trustedAccounts = legalEntitiesWithPermissionResponse.AccountProviderLegalEntities
                .DistinctBy(x => x.AccountId)
                .ToDictionary(x => x.AccountId);

            user.Identities.First().AddClaim(new Claim(ProviderClaims.AssociatedAccountsClaimsTypeIdentifier, JsonConvert.SerializeObject(trustedAccounts), JsonClaimValueTypes.Json));
        }
        else
        {
            logger.LogInformation("{TypeName} trusted account claims found.", nameof(AccessCohortAuthorizationHelper));

            try
            {
                trustedAccounts = JsonConvert.DeserializeObject<Dictionary<long, GetProviderAccountLegalEntityWithCreatCohortItem>>(trustedAccountClaim);
            }
            catch (JsonSerializationException exception)
            {
                logger.LogError(exception, "{TypeName} Could not deserialize trusted accounts claim for provider.", nameof(AccessCohortAuthorizationHelper));
                return false;
            }
        }

        var accountLegalEntityId = encodingService.Decode(accountLegalEntityPublicHashedId?.ToString(), EncodingType.PublicAccountLegalEntityId);

        logger.LogInformation("{TypeName} trusted accounts {trustedAccounts}.", nameof(AccessCohortAuthorizationHelper), JsonConvert.SerializeObject(trustedAccounts));
        logger.LogInformation("{TypeName} accountLegalEntityId from Route: {accountLegalEntityId}.", nameof(AccessCohortAuthorizationHelper), accountLegalEntityId);

        var accountLegalEntityIdFound = trustedAccounts.ContainsKey(accountLegalEntityId);

        logger.LogInformation("{TypeName} accountLegalEntityIdFound: {accountLegalEntityIdFound}.", nameof(AccessCohortAuthorizationHelper), accountLegalEntityIdFound);

        return accountLegalEntityIdFound;
    }
}