using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SFA.DAS.Authorization.CommitmentPermissions.Configuration;
using SFA.DAS.EAS.Account.Api.Client;
using SFA.DAS.Encoding;
using SFA.DAS.Reservations.Infrastructure.Configuration;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Infrastructure.Services;
using SFA.DAS.Reservations.Web.Infrastructure;
using AccountApiConfiguration = SFA.DAS.Reservations.Infrastructure.Configuration.AccountApiConfiguration;

namespace SFA.DAS.Reservations.Web.AppStart
{
    public static class ConfigurationExtensions
    {
        public static void AddEmployerConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<IdentityServerConfiguration>(configuration.GetSection("Identity"));
            services.AddSingleton(cfg => cfg.GetService<IOptions<IdentityServerConfiguration>>().Value);
            
            services.AddSingleton<IAuthorizationHandler, EmployerAccountAuthorizationHandler>();

            AddSharedConfiguration(services, configuration);
        }

        public static void AddProviderConfiguration(this IServiceCollection services, IConfiguration configuration)
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
            var encodingConfigJson = configuration.GetSection(nameof(EncodingConfig)).Value;
            var encodingConfig = JsonConvert.DeserializeObject<EncodingConfig>(encodingConfigJson);
            services.AddSingleton(encodingConfig);

            services.Configure<ReservationsApiConfiguration>(configuration.GetSection("ReservationsApi"));
            services.AddSingleton(config => config.GetService<IOptions<ReservationsApiConfiguration>>().Value);
            services.Configure<ReservationsWebConfiguration>(configuration.GetSection("ReservationsWeb"));
            services.AddSingleton(config => config.GetService<IOptions<ReservationsWebConfiguration>>().Value);
            services.Configure<AccountApiConfiguration>(configuration.GetSection("AccountApi"));

            services.Configure<CommitmentPermissionsApiClientConfiguration>(configuration.GetSection("CommitmentsApiClient"));
            services.AddSingleton(config => config.GetService<IOptions<CommitmentPermissionsApiClientConfiguration>>().Value);

            services.Configure<CommitmentsApiConfiguration>(configuration.GetSection("CommitmentsApiClient"));
            services.AddSingleton(config => config.GetService<IOptions<CommitmentsApiConfiguration>>().Value);

            services.AddSingleton<IAccountApiConfiguration, AccountApiConfiguration>(cfg => cfg.GetService<IOptions<AccountApiConfiguration>>().Value);
            services.AddTransient<IAccountApiClient, AccountApiClient>();
            services.AddTransient<IEmployerAccountService, EmployerAccountService>();

            services.AddSingleton<IAuthorizationHandler, HasProviderOrEmployerAccountAuthorisationHandler>();
            services.AddSingleton<IProviderAuthorisationHandler, ProviderAuthorizationHandler>();
            services.AddSingleton<IEmployerAccountAuthorisationHandler, EmployerAccountAuthorizationHandler>();
            services.AddSingleton<IAuthorizationHandler, HasTransactorOrOwnerUserRoleAuthorisationHandler>();

        }
    }
}
