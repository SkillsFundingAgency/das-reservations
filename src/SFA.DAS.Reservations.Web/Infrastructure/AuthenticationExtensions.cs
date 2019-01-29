using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SFA.DAS.Reservations.Models.Configuration;
using SFA.DAS.Reservations.Web.Services;

namespace SFA.DAS.Reservations.Web.Infrastructure
{
    public static class AuthenticationExtensions
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
            });

            services.AddSingleton<IAuthorizationHandler, EmployerAccountHandler>();
        }

        public static void AddAndConfigureAuthentication(
            this IServiceCollection services,
            IOptions<IdentityServerConfiguration> configuration, 
            IEmployerAccountService accountsSvc)
        {
            services
                .AddAuthentication(sharedOptions =>
                {
                    sharedOptions.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    sharedOptions.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    sharedOptions.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
                    sharedOptions.DefaultSignOutScheme = OpenIdConnectDefaults.AuthenticationScheme;

                }).AddOpenIdConnect(options =>
                {
                    options.ClientId = configuration.Value.ClientId;
                    options.ClientSecret = configuration.Value.ClientSecret;
                    options.AuthenticationMethod = OpenIdConnectRedirectBehavior.RedirectGet;
                    options.Authority = configuration.Value.BaseAddress;
                    options.ResponseType = configuration.Value.ResponseType;
                    options.SaveTokens = configuration.Value.SaveTokens;
                    options.GetClaimsFromUserInfoEndpoint = false;
                    var scopes = configuration.Value.Scopes.Split(' ');
                    foreach (var scope in scopes)
                    {
                        options.Scope.Add(scope);
                    }

                    //var mapUniqueJsonKeys = configuration.Value.MapUniqueJsonKey.Split(' ');
                    //options.ClaimActions.MapUniqueJsonKey(mapUniqueJsonKeys[0], mapUniqueJsonKeys[1]);
                    options.Events.OnTokenValidated = async (ctx) => await PopulateAccountsClaim(ctx, accountsSvc);

                })
                .AddCookie(options =>
                {
                    options.AccessDeniedPath = new PathString("/Service/AccessDenied");
                    options.ExpireTimeSpan = TimeSpan.FromHours(1);
                    options.Events.OnRedirectToAccessDenied = RedirectToAccessDenied;
                });
        }

        private static async Task PopulateAccountsClaim(TokenValidatedContext ctx, IEmployerAccountService accountsSvc)
        {
            var userId = ctx.Principal.Claims
                .First(c => c.Type.Equals(EmployerClaims.IdamsUserIdClaimTypeIdentifier))
                .Value;
            var associatedAccountsClaim = await accountsSvc.GetClaim(userId);
            ctx.Principal.Identities.First().AddClaim(associatedAccountsClaim);
        }

        private static Task RedirectToAccessDenied(RedirectContext<CookieAuthenticationOptions> context)
        {
            var routeData = context.HttpContext.GetRouteData();
            var path = context.Request.Path.Value;
            path = path.EndsWith("/") ? path.Substring(0, path.Length - 1) : path;

            if (path.Contains("Home/Index") || path.Equals($"/Accounts/{routeData.Values["employerAccountId"]}"))
            {
                //default path
                context.Response.Redirect(context.RedirectUri);
            }
            else
            { 
                //custom
                context.Response.Redirect(context.Request.PathBase + $"/Accounts/{routeData.Values["employerAccountId"]}/Home/Index");
            }

            return Task.CompletedTask;
        }
    }
}
