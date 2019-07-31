using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication.WsFederation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SFA.DAS.Provider.Idams.Stub.Extensions;
using SFA.DAS.Reservations.Infrastructure.Configuration;
using SFA.DAS.Reservations.Web.Infrastructure;

namespace SFA.DAS.Reservations.Web.AppStart
{
    public static class AuthenticationProviderExtensions
    {
        public static void AddAndConfigureProviderAuthentication(this IServiceCollection services, IOptions<ProviderIdamsConfiguration> configuration)
        {
            var cookieOptions = new Action<CookieAuthenticationOptions>(options =>
            {
                options.CookieManager = new ChunkingCookieManager {ChunkSize = 3000};
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.AccessDeniedPath = "/error/403";
            });

            if (configuration.Value != null && configuration.Value.UseStub)
            {
                services.AddProviderIdamsStubAuthentication(cookieOptions, new OpenIdConnectEvents
                {
                    OnTokenValidated = async (ctx) =>
                    {
                        await PopulateProviderClaims(ctx.HttpContext, ctx.Principal);
                    }
                });
            }
            else
            {
                services.AddAuthentication(sharedOptions =>
                    {
                        sharedOptions.DefaultScheme =
                            CookieAuthenticationDefaults.AuthenticationScheme;
                        sharedOptions.DefaultSignInScheme =
                            CookieAuthenticationDefaults.AuthenticationScheme;
                        sharedOptions.DefaultChallengeScheme =
                            WsFederationDefaults.AuthenticationScheme;
                    })
                    .AddWsFederation(options =>
                    {
                        options.MetadataAddress = configuration.Value.MetadataAddress;
                        options.Wtrealm = configuration.Value.Wtrealm;
                        options.CallbackPath = "/{ukprn}/reservations";
                        options.Events.OnSecurityTokenValidated = async (ctx) =>
                        {
                            await PopulateProviderClaims(ctx.HttpContext, ctx.Principal);
                        };
                    }).AddCookie(cookieOptions);
            }
        }

        private static Task PopulateProviderClaims(HttpContext httpContext, ClaimsPrincipal principal)
        {
            var providerId = principal.Claims.First(c => c.Type.Equals(ProviderClaims.ProviderUkprn)).Value;
            var displayName = principal.Claims.First(c => c.Type.Equals(ProviderClaims.DisplayName)).Value;
            httpContext.Items.Add(ClaimsIdentity.DefaultNameClaimType,providerId);
            httpContext.Items.Add(ProviderClaims.DisplayName,displayName);
            principal.Identities.First().AddClaim(new Claim(ClaimsIdentity.DefaultNameClaimType, providerId));
            principal.Identities.First().AddClaim(new Claim(ProviderClaims.DisplayName, displayName));
            return Task.CompletedTask;
        }
    }
}