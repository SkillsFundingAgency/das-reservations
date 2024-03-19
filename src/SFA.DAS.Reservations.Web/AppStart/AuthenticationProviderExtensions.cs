using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication.WsFederation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using SFA.DAS.Provider.Idams.Stub.Extensions;
using SFA.DAS.ProviderRelationships.Types.Models;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Infrastructure.Configuration;
using SFA.DAS.Reservations.Web.Infrastructure;
using SFA.DAS.Reservations.Web.Stubs;

namespace SFA.DAS.Reservations.Web.AppStart;

public static class AuthenticationProviderExtensions
{
    public static void AddAndConfigureProviderAuthentication(
        this IServiceCollection services,
        ProviderIdamsConfiguration idamsConfiguration,
        IConfiguration config,
        IHostEnvironment env,
        IReservationsOuterService outerService)
    {
        var cookieOptions = new Action<CookieAuthenticationOptions>(options =>
        {
            options.CookieManager = new ChunkingCookieManager { ChunkSize = 3000 };
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.AccessDeniedPath = "/error/403";
        });

        if (env.IsDevelopment() && config.UseStubs())
        {
            services.AddProviderIdamsStubAuthentication(cookieOptions, new OpenIdConnectEvents
            {
                OnTokenValidated = async ctx => { await PopulateProviderClaims(ctx.HttpContext, ctx.Principal, outerService); }
            });
        }
        else
        {
            services.AddAuthentication(sharedOptions =>
                {
                    sharedOptions.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    sharedOptions.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    sharedOptions.DefaultChallengeScheme = WsFederationDefaults.AuthenticationScheme;
                })
                .AddWsFederation(options =>
                {
                    options.MetadataAddress = idamsConfiguration.MetadataAddress;
                    options.Wtrealm = idamsConfiguration.Wtrealm;
                    options.CallbackPath = "/{ukprn}/reservations";
                    options.Events.OnSecurityTokenValidated = async (ctx) => { await PopulateProviderClaims(ctx.HttpContext, ctx.Principal, outerService); };
                }).AddCookie(cookieOptions);
        }
    }

    private static async Task PopulateProviderClaims(HttpContext httpContext, ClaimsPrincipal principal, IReservationsOuterService outerService)
    {
        var providerIdValue = principal.Claims.First(c => c.Type.Equals(ProviderClaims.ProviderUkprn)).Value;
        var displayName = principal.Claims.First(c => c.Type.Equals(ProviderClaims.DisplayName)).Value;

        httpContext.Items.Add(ClaimsIdentity.DefaultNameClaimType, providerIdValue);
        httpContext.Items.Add(ProviderClaims.DisplayName, displayName);

        principal.Identities.First().AddClaim(new Claim(ClaimsIdentity.DefaultNameClaimType, providerIdValue));
        principal.Identities.First().AddClaim(new Claim(ProviderClaims.DisplayName, displayName));

        var providerId = int.Parse(providerIdValue);
        var legalEntitiesWithPermissionResponse = await outerService.GetAccountProviderLegalEntitiesWithPermission(providerId, Operation.CreateCohort);
        var trustedEmployers = legalEntitiesWithPermissionResponse.AccountProviderLegalEntities.ToDictionary(x => x.Id);
        var trustedEmployersAsJson = JsonConvert.SerializeObject(trustedEmployers);
        
        principal.Identities.First().AddClaim(new Claim(ProviderClaims.TrustedEmployerAccounts, trustedEmployersAsJson, JsonClaimValueTypes.Json));
    }
}