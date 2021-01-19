using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.Reservations.Infrastructure.Configuration;
using StackExchange.Redis;

namespace SFA.DAS.Reservations.Web.Extensions
{
    public static class DataProtectionStartupExtensions
    {
        public static IServiceCollection AddDataProtection(this IServiceCollection services, IConfiguration configuration, IHostingEnvironment environment)
        {
            if (!environment.IsDevelopment())
            {
                var config = configuration.GetSection("ReservationsWebConfiguration").Get<ReservationsWebConfiguration>();

                if (config != null)
                {
                    var redisConnectionString = config.RedisCacheConnectionString;
                    var dataProtectionKeysDatabase = config.DataProtectionKeysDatabase;

                    var redis = ConnectionMultiplexer
                        .Connect($"{redisConnectionString},{dataProtectionKeysDatabase}");

                    services.AddDataProtection()
                        .SetApplicationName("das-reservations-web")
                        .PersistKeysToStackExchangeRedis(redis, "DataProtection-Keys");
                }
            }

            return services;
        }
    }
}
