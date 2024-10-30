using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SFA.DAS.DfESignIn.Auth.AppStart;
using SFA.DAS.DfESignIn.Auth.Enums;
using SFA.DAS.GovUK.Auth.AppStart;
using SFA.DAS.GovUK.Auth.Configuration;
using SFA.DAS.Reservations.Infrastructure.Configuration;
using SFA.DAS.Reservations.Web.AppStart;
using SFA.DAS.Reservations.Web.Infrastructure.Authorization;
using System;

namespace SFA.DAS.Reservations.Web.Extensions;

public static class AuthenticationStartupExtensions
{
    public static IServiceCollection SetupProviderAuth(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        // This ensures the way claims are mapped are consistent with version 7 of OpenIdConnect
        // but this only applies on the Provider side
        Microsoft.IdentityModel.JsonWebTokens.JsonWebTokenHandler.DefaultInboundClaimTypeMap.Clear();

        if (configuration["ReservationsWeb:UseDfESignIn"] != null && configuration["ReservationsWeb:UseDfESignIn"].Equals("true", StringComparison.CurrentCultureIgnoreCase))
        {
            services.AddAndConfigureDfESignInAuthentication(
                configuration,
                "SFA.DAS.ProviderApprenticeshipService",
                typeof(CustomServiceRole),
                ClientName.ProviderRoatp,
                "/signout",
                "");
        }
        else
        {
            var providerIdamsConfiguration = configuration
                .GetSection("ProviderIdams")
                .Get<ProviderIdamsConfiguration>();
            
            
            services.AddAndConfigureProviderAuthentication(providerIdamsConfiguration,
                configuration,
                environment);
        }

        return services;
    }

    public static IServiceCollection SetupEmployerAuth(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<GovUkOidcConfiguration>(configuration.GetSection("GovUkOidcConfiguration"));
        services.AddSingleton(config => config.GetService<IOptions<GovUkOidcConfiguration>>().Value);
        services.AddAndConfigureGovUkAuthentication(configuration, typeof(EmployerAccountPostAuthenticationClaimsHandler), "", "/SignIn-Stub");
        return services;
    }
}