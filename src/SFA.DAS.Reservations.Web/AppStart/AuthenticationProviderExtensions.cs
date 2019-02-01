using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.WsFederation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SFA.DAS.Reservations.Infrastructure.Configuration.Configuration;

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
                    options.CallbackPath = "/Home/Index";
                    options.Events.OnSecurityTokenValidated =  async (ctx) =>
                    {
                        await PopulateProviderClaims(ctx);
                    };
                    
                }).AddCookie(options => { options.ReturnUrlParameter = "/Home/Index"; }); ;
        }

        private static Task PopulateProviderClaims(SecurityTokenValidatedContext ctx)
        {
            var providerId = ctx.Principal.Claims;
            
            return Task.CompletedTask;
        }
    }
}