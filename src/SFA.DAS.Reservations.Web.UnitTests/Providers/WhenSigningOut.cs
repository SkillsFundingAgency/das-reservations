using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using FluentAssertions;
using SFA.DAS.Reservations.Infrastructure.Configuration;
using SFA.DAS.Reservations.Web.Controllers;
using AutoFixture.NUnit3;
using Microsoft.Extensions.Configuration;
using Moq;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Reservations.Web.UnitTests.Providers;

[TestFixture]
public class WhenSigningOut
{
    [Test, MoqAutoData]
    public void Then_It_Signs_ProviderUserOut_With_DfESignIn(
        string redirectUrl,
        [Frozen] Mock<IConfiguration> rootConfig,
        [Frozen] ReservationsWebConfiguration configuration,
        [NoAutoProperties] HomeController controller)
    {
        // Arrange
        rootConfig.Setup(x => x["AuthType"]).Returns("provider");
        configuration.DashboardUrl = redirectUrl;

        // Act
        var result = controller.SignOut() as SignOutResult;
        
        // Assert
        result.Should().NotBeNull();
        result.AuthenticationSchemes.Should().Contain(CookieAuthenticationDefaults.AuthenticationScheme);
        result.AuthenticationSchemes.Should().Contain(OpenIdConnectDefaults.AuthenticationScheme);
    }
    
    [Test, MoqAutoData]
    public void Then_It_Signs_EmployerUserOut_With_StubAuth(
        string redirectUrl,
        [Frozen] Mock<IConfiguration> rootConfig,
        [Frozen] ReservationsWebConfiguration configuration,
        [NoAutoProperties] HomeController controller)
    {
        // Arrange
        rootConfig.Setup(x => x["AuthType"]).Returns("employer");
        rootConfig.Setup(x => x["StubAuth"]).Returns("true");
        configuration.DashboardUrl = redirectUrl;

        // Act
        var result = controller.SignOut() as SignOutResult;
        
        // Assert
        result.Should().NotBeNull();
        result.AuthenticationSchemes.Should().Contain(CookieAuthenticationDefaults.AuthenticationScheme);
        result.AuthenticationSchemes.Should().NotContain(OpenIdConnectDefaults.AuthenticationScheme);
    }

    [Test, MoqAutoData]
    public void Then_It_Signs_EmployerUserOut_With_OpenIdConnect(
        string redirectUrl,
        [Frozen] Mock<IConfiguration> rootConfig,
        [Frozen] ReservationsWebConfiguration configuration,
        [NoAutoProperties] HomeController controller)
    {
        // Arrange
        rootConfig.Setup(x => x["AuthType"]).Returns("employer");
        rootConfig.Setup(x => x["StubAuth"]).Returns("false");
        configuration.DashboardUrl = redirectUrl;

        // Act
        var result = controller.SignOut() as SignOutResult;
        
        // Assert
        result.Should().NotBeNull();
        result.AuthenticationSchemes.Should().Contain(CookieAuthenticationDefaults.AuthenticationScheme);
        result.AuthenticationSchemes.Should().Contain(OpenIdConnectDefaults.AuthenticationScheme);
    }
}