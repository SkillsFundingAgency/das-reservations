using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.WsFederation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SFA.DAS.Reservations.Infrastructure.Configuration;
using SFA.DAS.Reservations.Web.Infrastructure;

namespace SFA.DAS.Reservations.Web.AppStart
{
    public static class AuthenticationProviderExtensions
    {
        public static void AddAndConfigureProviderAuthentication(this IServiceCollection services, IOptions<ProviderIdamsConfiguration> configuration)
        {
            
            services.AddAuthentication(sharedOptions =>
                {
                    sharedOptions.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    sharedOptions.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    sharedOptions.DefaultChallengeScheme = WsFederationDefaults.AuthenticationScheme;
                })
                .AddWsFederation(options =>
                {
                    options.MetadataAddress = configuration.Value.MetadataAddress;
                    options.Wtrealm = configuration.Value.Wtrealm;
                    options.CallbackPath = "/{ukprn}/reservations";
                    options.Events.OnSecurityTokenValidated =  async (ctx) =>
                    {
                        await PopulateProviderClaims(ctx);
                    };
                }).AddCookie(options =>
                {
                    options.ReturnUrlParameter = "/{ukprn}/reservations";
                    options.AccessDeniedPath = "/error/403";
                }); ;
        }

        private static Task PopulateProviderClaims(SecurityTokenValidatedContext ctx)
        {
            var providerId = ctx.Principal.Claims.First(c => c.Type.Equals(ProviderClaims.ProviderUkprn)).Value;
            var displayName = ctx.Principal.Claims.First(c => c.Type.Equals(ProviderClaims.DisplayName)).Value;
            ctx.HttpContext.Items.Add(ClaimsIdentity.DefaultNameClaimType,providerId);
            ctx.HttpContext.Items.Add(ProviderClaims.DisplayName,displayName);
            ctx.Principal.Identities.First().AddClaim(new Claim(ClaimsIdentity.DefaultNameClaimType, providerId));
            ctx.Principal.Identities.First().AddClaim(new Claim(ProviderClaims.DisplayName, displayName));
            return Task.CompletedTask;
        }
    }
}