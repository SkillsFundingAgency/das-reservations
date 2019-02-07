using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
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
                    });
                options.AddPolicy(
                    PolicyNames
                        .HasProviderAccount
                    , policy =>
                    {
                        policy.RequireAuthenticatedUser();
                        policy.RequireClaim(ProviderClaims.ProviderUkprn);
                        policy.Requirements.Add(new ProviderUkPrnRequirement());
                    });
            });
        }
    }
}