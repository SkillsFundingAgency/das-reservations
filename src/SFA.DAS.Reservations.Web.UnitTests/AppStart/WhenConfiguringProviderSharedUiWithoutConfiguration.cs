using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using SFA.DAS.Provider.Shared.UI.Startup;

namespace SFA.DAS.Reservations.Web.UnitTests.AppStart;

public class WhenConfiguringProviderSharedUiWithoutConfiguration
{
    private IConfiguration _configuration;
    private IServiceCollection _services;

    [SetUp]
    public void Setup()
    {
        _services = new ServiceCollection();

        // Create a configuration WITHOUT ProviderSharedUIConfiguration section
        // This should cause AddProviderUiServiceRegistration to throw
        var configBuilder = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                // Intentionally missing ProviderSharedUIConfiguration section
            });

        _configuration = configBuilder.Build();
    }

    [Test]
    public void Then_Throws_Exception_When_ProviderSharedUIConfiguration_Missing()
    {
        // Act & Assert
        var exception = Assert.Throws<Exception>(() => _services.AddProviderUiServiceRegistration(_configuration));
        exception.Message.Should().Contain("Cannot find ProviderSharedUIConfiguration in configuration");
    }
}

