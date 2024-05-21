using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using SFA.DAS.ProviderRelationships.Api.Client;
using SFA.DAS.ProviderRelationships.Api.Client.Configuration;
using SFA.DAS.ProviderRelationships.Api.Client.DependencyResolution.Microsoft;
using SFA.DAS.Reservations.Web.Stubs;

namespace SFA.DAS.Reservations.Web.AppStart;

public static class ProviderRelationshipsExtensions
{
    public static void AddProviderRelationsApi(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment env)
    {
        if (env.IsDevelopment() && configuration.UseStubs())
        {
            services.AddScoped<IProviderRelationshipsApiClient, ProviderRelationshipsApiClientStub>();
        }
        else
        {
            services.AddProviderRelationshipsApiClient();

            services.Configure<ProviderRelationshipsApiConfiguration>(configuration.GetSection("ProviderRelationshipsApi"));
            services.AddSingleton(serviceProvider => serviceProvider.GetService<IOptions<ProviderRelationshipsApiConfiguration>>().Value);
        }
    }
}