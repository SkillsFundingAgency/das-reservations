using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.Reservations.Infrastructure.Api;

namespace SFA.DAS.Reservations.Web.AppStart;

public static class ProviderRelationshipsExtensions
{
    public static void AddProviderRelationshipsOuterApi(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment env)
    {
        services.AddTransient<IProviderRelationshipsOuterApiClient, ProviderRelationshipsOuterApiClient>();
    }
}