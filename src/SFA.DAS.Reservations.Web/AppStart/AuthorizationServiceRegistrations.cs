using System;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.GovUK.Auth.Authentication;
using SFA.DAS.Reservations.Infrastructure.Services;
using SFA.DAS.Reservations.Web.Infrastructure;
using SFA.DAS.Reservations.Web.Infrastructure.Authorization;

namespace SFA.DAS.Reservations.Web.AppStart;

public static class AuthorizationServiceRegistrations
{
    public static void AddAuthorizationServices(this IServiceCollection services)
    {
        // This ensures the way claims are mapped are consistent with version 7 of OpenIdConnect
        Microsoft.IdentityModel.JsonWebTokens.JsonWebTokenHandler.DefaultInboundClaimTypeMap.Clear();

        services.AddAuthorization(options =>
        {
            options.AddPolicy(PolicyNames.HasEmployerAccount, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireClaim(EmployerClaims.AccountsClaimsTypeIdentifier);
                policy.Requirements.Add(new EmployerAccountRequirement());
                policy.Requirements.Add(new AccountActiveRequirement());
            });

            options.AddPolicy(PolicyNames.HasProviderAccount, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireClaim(ProviderClaims.ProviderUkprn);
                policy.RequireAssertion(HasValidServiceClaim);
                policy.Requirements.Add(new ProviderUkPrnRequirement());
                policy.Requirements.Add(new TrainingProviderAllRolesRequirement());
            });

            options.AddPolicy(PolicyNames.HasProviderOrEmployerAccount, policy =>
            {
                policy.RequireAuthenticatedUser();
                ProviderOrEmployerAssertion(policy);
                policy.Requirements.Add(new HasProviderOrEmployerAccountRequirement());
                policy.Requirements.Add(new AccountActiveRequirement());
                policy.Requirements.Add(new TrainingProviderAllRolesRequirement());
            });

            options.AddPolicy(PolicyNames.HasEmployerViewerUserRoleOrIsProvider, policy =>
            {
                policy.RequireAuthenticatedUser();
                ProviderOrEmployerAssertion(policy);
                policy.Requirements.Add(new HasEmployerViewerUserRoleOrIsProviderRequirement());
                policy.Requirements.Add(new AccountActiveRequirement());
                policy.Requirements.Add(new TrainingProviderAllRolesRequirement());
            });

            options.AddPolicy(PolicyNames.HasProviderGotViewerOrHigherRoleOrIsEmployer, policy =>
            {
                policy.RequireAuthenticatedUser();
                ProviderOrEmployerAssertion(policy);
                policy.Requirements.Add(new MinimumServiceClaimRequirement(ServiceClaim.DAV));
                policy.Requirements.Add(new AccountActiveRequirement());
                policy.Requirements.Add(new TrainingProviderAllRolesRequirement());
            });

            options.AddPolicy(PolicyNames.HasProviderGotContributorOrHigherRoleOrIsEmployer, policy =>
            {
                policy.RequireAuthenticatedUser();
                ProviderOrEmployerAssertion(policy);
                policy.Requirements.Add(new MinimumServiceClaimRequirement(ServiceClaim.DAC));
                policy.Requirements.Add(new AccountActiveRequirement());
                policy.Requirements.Add(new TrainingProviderAllRolesRequirement());
            });
            
            options.AddPolicy(PolicyNames.AccessCohort, policy =>
            {
                policy.RequireAuthenticatedUser();
                ProviderOrEmployerAssertion(policy);
                policy.Requirements.Add(new AccessCohortRequirement());
            });
        });
    }

    private static void ProviderOrEmployerAssertion(AuthorizationPolicyBuilder policy)
    {
        policy.RequireAssertion(context =>
        {
            var hasUkprn = context.User.HasClaim(claim => claim.Type.Equals(ProviderClaims.ProviderUkprn));
            var hasDaa = HasValidServiceClaim(context);

            var hasEmployerAccountId = context.User.HasClaim(claim => claim.Type.Equals(EmployerClaims.AccountsClaimsTypeIdentifier));
            return hasUkprn && hasDaa || hasEmployerAccountId;
        });
    }

    private static bool HasValidServiceClaim(AuthorizationHandlerContext context)
    {
        var validClaimsList = Enum.GetNames(typeof(ServiceClaim)).ToList();

        return context.User.HasClaim(claim =>
            claim.Type.Equals(ProviderClaims.Service) &&
            validClaimsList.Any(x => claim.Value.Equals(x)));
    }
}