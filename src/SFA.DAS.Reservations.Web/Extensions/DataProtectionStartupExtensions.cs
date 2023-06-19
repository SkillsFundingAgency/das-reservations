using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.Reservations.Infrastructure.Configuration;
using StackExchange.Redis;
using Microsoft.Extensions.Hosting;

namespace SFA.DAS.Reservations.Web.Extensions
{
    public static class DataProtectionStartupExtensions
    {
        public static IServiceCollection AddDataProtection(this IServiceCollection services, ReservationsWebConfiguration configuration, IWebHostEnvironment environment, bool isEmployerAuth)
        {
            if (!environment.IsDevelopment())
            {
                if (configuration != null)
                {
                    var redisConnectionString = configuration.RedisCacheConnectionString;
                    var dataProtectionKeysDatabase = configuration.DataProtectionKeysDatabase;

                    var redis = ConnectionMultiplexer
                        .Connect($"{redisConnectionString},{dataProtectionKeysDatabase}");

                    var applicationName = isEmployerAuth ? "das-employer" : "das-provider";
                    
                    services.AddDataProtection()
                        .SetApplicationName(applicationName)
                        .PersistKeysToStackExchangeRedis(redis, "DataProtection-Keys");
                }
            }

            return services;
        }
    }
}
