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
        public static IServiceCollection AddDataProtection(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
        {
            if (!environment.IsDevelopment())
            {
                var config = services.BuildServiceProvider().GetService<ReservationsWebConfiguration>();

                if (config != null)
                {
                    var redisConnectionString = config.RedisCacheConnectionString;
                    var dataProtectionKeysDatabase = config.DataProtectionKeysDatabase;

                    var redis = ConnectionMultiplexer
                        .Connect($"{redisConnectionString},{dataProtectionKeysDatabase}");

                    services.AddDataProtection()
                        .SetApplicationName("das-employer")
                        .PersistKeysToStackExchangeRedis(redis, "DataProtection-Keys");
                }
            }

            return services;
        }
    }
}
