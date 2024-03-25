using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication.WsFederation;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SFA.DAS.Provider.Idams.Stub.Extensions;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Infrastructure.Configuration;
using SFA.DAS.Reservations.Web.Handlers;
using SFA.DAS.Reservations.Web.Stubs;

namespace SFA.DAS.Reservations.Web.AppStart;

public static class AuthenticationProviderExtensions
{
    public static void AddAndConfigureProviderAuthentication(
        this IServiceCollection services,
        ProviderIdamsConfiguration idamsConfiguration,
        IConfiguration config,
        IHostEnvironment env)
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
                OnTokenValidated = async ctx => { await PopulateProviderClaims(ctx.HttpContext, ctx.Principal); }
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
                    options.Events.OnSecurityTokenValidated = async (ctx) => { await PopulateProviderClaims(ctx.HttpContext, ctx.Principal); };
                }).AddCookie(cookieOptions);
        }
    }

    private static async Task PopulateProviderClaims(HttpContext httpContext, ClaimsPrincipal principal)
    {
        var outerService = httpContext.RequestServices.GetService<IReservationsOuterService>();
        var claimsHandler = new ProviderAccountPostAuthenticationClaimsHandler(outerService);
        
        await claimsHandler.GetClaims(httpContext, principal);
    }
}