﻿using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using SFA.DAS.Configuration.AzureTableStorage;

namespace SFA.DAS.Reservations.Web.Extensions;

public static class AuthTypes
{
    public const string Employer = nameof(Employer);
    public const string Provider = nameof(Provider);
}

public static class ConfigurationExtensions
{
    private const string EncodingConfigKey = "SFA.DAS.Encoding";
    public static bool IsEmployerAuth(this IConfiguration configuration) => configuration["AuthType"].Equals(AuthTypes.Employer, StringComparison.CurrentCultureIgnoreCase);
    public static bool IsProviderAuth(this IConfiguration configuration) => configuration["AuthType"].Equals(AuthTypes.Provider, StringComparison.CurrentCultureIgnoreCase);
    
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