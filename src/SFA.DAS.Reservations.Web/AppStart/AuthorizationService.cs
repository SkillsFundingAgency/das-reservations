using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.GovUK.Auth.Authentication;
using SFA.DAS.Reservations.Web.Infrastructure;

namespace SFA.DAS.Reservations.Web.AppStart
{
    public static class AuthorizationService
    {
        public static void AddAuthorizationService(this IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                options.AddPolicy(
                    PolicyNames
                        .HasEmployerAccount
                    , policy =>
                    {
                        policy.RequireAuthenticatedUser();
                        policy.RequireClaim(EmployerClaims.AccountsClaimsTypeIdentifier);
                        policy.Requirements.Add(new EmployerAccountRequirement());
                        policy.Requirements.Add(new AccountActiveRequirement());
                    });
                options.AddPolicy(
                    PolicyNames
                        .HasProviderAccount
                    , policy =>
                    {
                        policy.RequireAuthenticatedUser();
                        policy.RequireClaim(ProviderClaims.ProviderUkprn);
                        policy.RequireAssertion(HasValidServiceClaim);
                        policy.Requirements.Add(new ProviderUkPrnRequirement());
                    });
                options.AddPolicy(
                    PolicyNames.HasProviderOrEmployerAccount, policy =>
                    {
                        policy.RequireAuthenticatedUser();
                        ProviderOrEmployerAssertion(policy);
                        policy.Requirements.Add(new HasProviderOrEmployerAccountRequirement());
                        policy.Requirements.Add(new AccountActiveRequirement());
                    });
                options.AddPolicy(
                    PolicyNames.HasEmployerViewerUserRoleOrIsProvider
                    , policy =>
                    {
                        policy.RequireAuthenticatedUser();
                        ProviderOrEmployerAssertion(policy);
                        policy.Requirements.Add(new HasEmployerViewerUserRoleOrIsProviderRequirement());
                        policy.Requirements.Add(new AccountActiveRequirement());
                    });
                options.AddPolicy(
                    PolicyNames.HasProviderGotViewerOrHigherRoleOrIsEmployer
                    , policy =>
                    {
                        policy.RequireAuthenticatedUser();
                        ProviderOrEmployerAssertion(policy);
                        policy.Requirements.Add(new MinimumServiceClaimRequirement(ServiceClaim.DAV));
                        policy.Requirements.Add(new AccountActiveRequirement());
                    });
                options.AddPolicy(
                   PolicyNames.HasProviderGotContributorOrHigherRoleOrIsEmployer
                   , policy =>
                   {
                       policy.RequireAuthenticatedUser();
                       ProviderOrEmployerAssertion(policy);
                       policy.Requirements.Add(new MinimumServiceClaimRequirement(ServiceClaim.DAC));
                       policy.Requirements.Add(new AccountActiveRequirement());
                   });
            });
        }

        private static void ProviderOrEmployerAssertion(AuthorizationPolicyBuilder policy)
        {
            policy.RequireAssertion(context =>
            {
                var hasUkprn = context.User.HasClaim(claim =>
                    claim.Type.Equals(ProviderClaims.ProviderUkprn));
                var hasDaa = HasValidServiceClaim(context);

                var hasEmployerAccountId = context.User.HasClaim(claim =>
                    claim.Type.Equals(EmployerClaims.AccountsClaimsTypeIdentifier));
                return hasUkprn && hasDaa || hasEmployerAccountId;
            });
        }

        private static bool HasValidServiceClaim(AuthorizationHandlerContext context)
        {
            var validClaimsList =  Enum.GetNames(typeof(ServiceClaim)).ToList();
            var hasValidClaim = context.User.HasClaim(claim =>
                claim.Type.Equals(ProviderClaims.Service) &&
                validClaimsList.Any(x => claim.Value.Equals(x)));

            return hasValidClaim;
        }
    }
}