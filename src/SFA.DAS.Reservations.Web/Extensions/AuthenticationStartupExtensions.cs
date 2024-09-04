using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SFA.DAS.DfESignIn.Auth.AppStart;
using SFA.DAS.DfESignIn.Auth.Enums;
using SFA.DAS.GovUK.Auth.AppStart;
using SFA.DAS.GovUK.Auth.Configuration;
using SFA.DAS.Reservations.Infrastructure.Configuration;
using SFA.DAS.Reservations.Web.AppStart;
using SFA.DAS.Reservations.Web.Infrastructure;
using SFA.DAS.Reservations.Web.Infrastructure.Authorization;

namespace SFA.DAS.Reservations.Web.Extensions;

public static class AuthenticationStartupExtensions
{
    public static IServiceCollection SetupProviderAuth(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
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
        services.AddAndConfigureGovUkAuthentication(configuration, typeof(EmployerAccountPostAuthenticationClaimsHandler), "", "/SignIn-Stub");
        return services;
    }
}