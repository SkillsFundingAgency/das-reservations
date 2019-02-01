using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SFA.DAS.EAS.Account.Api.Client;
using SFA.DAS.Reservations.Infrastructure.Configuration.Configuration;
using SFA.DAS.Reservations.Web.Infrastructure;
using SFA.DAS.Reservations.Web.Services;
using AccountApiConfiguration = SFA.DAS.Reservations.Infrastructure.Configuration.Configuration.AccountApiConfiguration;

namespace SFA.DAS.Reservations.Web.AppStart
{
    public static class ConfigurationExtensions
    {
        public static void AddEmployerConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<IdentityServerConfiguration>(configuration.GetSection("Identity"));
            services.AddSingleton(cfg => cfg.GetService<IOptions<IdentityServerConfiguration>>().Value);
            services.Configure<AccountApiConfiguration>(configuration.GetSection("AccountApi"));
            services.AddSingleton<IAccountApiConfiguration, AccountApiConfiguration>(cfg => cfg.GetService<IOptions<AccountApiConfiguration>>().Value);
            services.AddTransient<IAccountApiClient, AccountApiClient>();
            services.AddTransient<IEmployerAccountService, EmployerAccountService>();
            services.AddSingleton<IAuthorizationHandler, EmployerAccountAuthorizationHandler>();
        }

        public static void AddProviderConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<ProviderIdamsConfiguration>(configuration.GetSection("ProviderIdams"));
            services.AddSingleton(cfg => cfg.GetService<IOptions<ProviderIdamsConfiguration>>().Value);
        }
    }
}
