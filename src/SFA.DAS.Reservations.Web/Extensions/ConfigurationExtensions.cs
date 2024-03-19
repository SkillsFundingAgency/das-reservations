using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using SFA.DAS.Configuration.AzureTableStorage;

namespace SFA.DAS.Reservations.Web.Extensions;

public static class ConfigurationExtensions
{
    private const string EncodingConfigKey = "SFA.DAS.Encoding";

    public static IConfiguration BuildDasConfiguration(this IConfiguration configuration)
    {
        var config = new ConfigurationBuilder()
            .AddConfiguration(configuration)
            .SetBasePath(Directory.GetCurrentDirectory())
#if DEBUG
            .AddJsonFile("appsettings.json", true)
            .AddJsonFile("appsettings.Development.json", true)
#endif
            .AddEnvironmentVariables();

        if (!configuration["Environment"].Equals("DEV", StringComparison.CurrentCultureIgnoreCase))
        {
            config.AddAzureTableStorage(options =>
                {
                    options.ConfigurationKeys = configuration["ConfigNames"].Split(",");
                    options.StorageConnectionString = configuration["ConfigurationStorageConnectionString"];
                    options.EnvironmentName = configuration["Environment"];
                    options.PreFixConfigurationKeys = false;
                    options.ConfigurationKeysRawJsonResult = [EncodingConfigKey];
                }
            );
        }

        return config.Build();
    }
}