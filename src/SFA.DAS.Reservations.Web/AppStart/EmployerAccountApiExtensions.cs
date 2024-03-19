using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SFA.DAS.EAS.Account.Api.Client;
using SFA.DAS.Encoding;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Infrastructure.Services;
using SFA.DAS.Reservations.Web.Stubs;
using AccountApiConfiguration = SFA.DAS.Reservations.Infrastructure.Configuration.AccountApiConfiguration;
using Microsoft.Extensions.Hosting;

namespace SFA.DAS.Reservations.Web.AppStart
{
    public static class EmployerAccountApiExtensions
    {
        public static void AddEmployerAccountApi(
            this IServiceCollection services, 
            IConfiguration configuration,
            IHostEnvironment env)
        {
            if (env.IsDevelopment() && configuration.UseStubs())
            {
                services.AddSingleton<IAccountApiClient, EmployerAccountApiClientStub>();
            }
            else
            {
                services.AddSingleton<IAccountApiConfiguration, AccountApiConfiguration>(cfg => cfg.GetService<IOptions<AccountApiConfiguration>>().Value);
                services.AddTransient<IAccountApiClient, AccountApiClient>();
            }
            services.AddTransient<IEmployerAccountService, EmployerAccountService>();
        }
    }
}