using System.Net.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SFA.DAS.ProviderRelationships.Api.Client;
using SFA.DAS.ProviderRelationships.Api.Client.Configuration;
using SFA.DAS.ProviderRelationships.Api.Client.Http;
using SFA.DAS.Reservations.Web.Stubs;
using HttpClientFactory = SFA.DAS.ProviderRelationships.Api.Client.Http.HttpClientFactory;

namespace SFA.DAS.Reservations.Web.AppStart
{
    public static class ProviderRelationshipsExtensions
    {
        public static void AddProviderRelationsApi(this IServiceCollection services, IConfiguration configuration,
            IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                services.AddScoped<IProviderRelationshipsApiClient, ProviderRelationshipsApiClientStub>();
            }
            else
            {
                services.AddScoped<IProviderRelationshipsApiClient, ProviderRelationshipsApiClient>();
                services.AddScoped<IRestHttpClient, RestHttpClient>();

                services.AddSingleton<HttpClient>(provider =>
                    new HttpClientFactory(provider.GetService<ProviderRelationshipsApiClientConfiguration>()
                        .AzureActiveDirectoryClient).CreateHttpClient());

                services.Configure<AzureActiveDirectoryClientConfiguration>(
                    configuration.GetSection("AzureActiveDirectoryClient"));
                services.AddSingleton(config => new ProviderRelationshipsApiClientConfiguration
                {
                    AzureActiveDirectoryClient =
                        config.GetService<IOptions<AzureActiveDirectoryClientConfiguration>>().Value
                });
            }
        }
    }
}
