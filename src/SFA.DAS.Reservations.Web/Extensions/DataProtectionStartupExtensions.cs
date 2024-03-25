using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SFA.DAS.Reservations.Infrastructure.Configuration;
using StackExchange.Redis;

namespace SFA.DAS.Reservations.Web.Extensions;

public static class DataProtectionStartupExtensions
{
    public static IServiceCollection AddDataProtection(this IServiceCollection services, ReservationsWebConfiguration configuration, IHostEnvironment environment, bool isEmployerAuth)
    {
        if (environment.IsDevelopment())
        {
            return services;
        }

        if (configuration == null)
        {
            return services;
        }
        
        var redisConnectionString = configuration.RedisCacheConnectionString;
        var dataProtectionKeysDatabase = configuration.DataProtectionKeysDatabase;

        var redis = ConnectionMultiplexer
            .Connect($"{redisConnectionString},{dataProtectionKeysDatabase}");

        var applicationName = isEmployerAuth ? "das-employer" : "das-provider";
                    
        services.AddDataProtection()
            .SetApplicationName(applicationName)
            .PersistKeysToStackExchangeRedis(redis, "DataProtection-Keys");

        return services;
    }
}