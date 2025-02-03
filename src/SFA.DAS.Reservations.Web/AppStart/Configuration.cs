using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SFA.DAS.Encoding;
using SFA.DAS.GovUK.Auth.Authentication;
using SFA.DAS.GovUK.Auth.Employer;
using SFA.DAS.GovUK.Auth.Services;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Infrastructure.Configuration;
using SFA.DAS.Reservations.Infrastructure.Services;
using SFA.DAS.Reservations.Web.Infrastructure;
using SFA.DAS.Reservations.Web.Infrastructure.Authorization;
using SFA.DAS.Reservations.Web.Infrastructure.Authorization.Helpers;
using SFA.DAS.Reservations.Web.Services;

namespace SFA.DAS.Reservations.Web.AppStart;

public static class ConfigurationExtensions
{
    private const string EncodingConfigKey = "SFA.DAS.Encoding";

    public static void AddEmployerConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IAuthorizationHandler, EmployerAccountAuthorizationHandler>();

        AddSharedConfiguration(services, configuration);
    }

    public static void AddProviderConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<ProviderIdamsConfiguration>(configuration.GetSection("ProviderIdams"));
        services.AddSingleton(cfg => cfg.GetService<IOptions<ProviderIdamsConfiguration>>().Value);

        services.AddSingleton<IAuthorizationHandler, ProviderAuthorizationHandler>();

        AddSharedConfiguration(services, configuration);
    }

    private static void AddSharedConfiguration(
        IServiceCollection services,
        IConfiguration configuration)
    {
        if (string.IsNullOrEmpty(configuration["IsIntegrationTest"]))
        {
            var encodingConfigJson = configuration.GetSection(EncodingConfigKey).Value;
            var encodingConfig = JsonConvert.DeserializeObject<EncodingConfig>(encodingConfigJson);
            services.AddSingleton(encodingConfig);
        }
        else
        {
            var encodingConfig = new EncodingConfig();
            services.AddSingleton(encodingConfig);
        }

        services.Configure<ReservationsApiConfiguration>(configuration.GetSection("ReservationsApi"));
        services.AddSingleton(config => config.GetService<IOptions<ReservationsApiConfiguration>>().Value);
        services.Configure<ReservationsWebConfiguration>(configuration.GetSection("ReservationsWeb"));
        services.AddSingleton(config => config.GetService<IOptions<ReservationsWebConfiguration>>().Value);

        services.Configure<ReservationsOuterApiConfiguration>(configuration.GetSection("ReservationsOuterApi"));
        services.AddSingleton(config => config.GetService<IOptions<ReservationsOuterApiConfiguration>>().Value);

        services.AddTransient<IGovAuthEmployerAccountService, EmployerAccountService>();
        services.AddTransient<IEmployerAccountService, EmployerAccountService>();

        services.AddSingleton<IAuthorizationHandler, HasProviderOrEmployerAccountAuthorisationHandler>();
        services.AddSingleton<IProviderAuthorisationHandler, ProviderAuthorizationHandler>();
        services.AddSingleton<IEmployerAccountAuthorisationHandler, EmployerAccountAuthorizationHandler>();
        services.AddSingleton<IAuthorizationHandler, HasEmployerViewerUserRoleOrIsProvider>();
        services.AddSingleton<IAuthorizationHandler, MinimumServiceClaimRequirementHandler>();
        services.AddSingleton<IAuthorizationHandler, AccountActiveAuthorizationHandler>();
        services.AddSingleton<IAuthorizationHandler, AccessCohortAuthorizationHandler>();

        services.AddSingleton<ICreateCohortAuthorizationHelper, CreateCohortAuthorizationHelper>();
        services.AddSingleton<IAccessCohortAuthorizationHelper, AccessCohortAuthorizationHelper>();

        services.AddTransient<ICustomClaims, EmptyCustomClaimsService>();
        services.AddTransient<IStubAuthenticationService, StubAuthenticationService>();

        services.AddSingleton<ITrainingProviderAuthorizationHandler, TrainingProviderAuthorizationHandler>();
        services.AddSingleton<IAuthorizationHandler, TrainingProviderAllRolesAuthorizationHandler>();
    }
}