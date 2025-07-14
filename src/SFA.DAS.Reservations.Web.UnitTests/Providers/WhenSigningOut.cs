using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
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
    public async Task Then_It_Signs_ProviderUserOut_With_DfESignIn(
        string redirectUrl,
        [Frozen] Mock<IConfiguration> rootConfig,
        [Frozen] ReservationsWebConfiguration configuration,
        [NoAutoProperties] HomeController controller)
    {
        // Arrange
        rootConfig.Setup(x => x["AuthType"]).Returns("provider");
        configuration.DashboardUrl = redirectUrl;
        controller.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
        
        var mockAuthService = new Mock<IAuthenticationService>();
        mockAuthService.Setup(x => x.SignOutAsync(It.IsAny<HttpContext>(), It.IsAny<string>(), It.IsAny<AuthenticationProperties>()))
            .Returns(Task.CompletedTask);
        
        var httpContext = new DefaultHttpContext
        {
            RequestServices = Mock.Of<IServiceProvider>(sp => 
                sp.GetService(typeof(IAuthenticationService)) == mockAuthService.Object)
        };
        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

        // Act
        await controller.SignOut();
        
        // Assert
        controller.TempData["AutoSignOut"].Should().Be(false);
        
        mockAuthService.Verify(x => x.SignOutAsync(
            It.IsAny<HttpContext>(), 
            CookieAuthenticationDefaults.AuthenticationScheme, 
            It.IsAny<AuthenticationProperties>()), Times.Once);
        mockAuthService.Verify(x => x.SignOutAsync(
            It.IsAny<HttpContext>(), 
            OpenIdConnectDefaults.AuthenticationScheme, 
            It.IsAny<AuthenticationProperties>()), Times.Once);
    }
    
    [Test, MoqAutoData]
    public async Task Then_It_Signs_EmployerUserOut_With_StubAuth(
        string redirectUrl,
        [Frozen] Mock<IConfiguration> rootConfig,
        [Frozen] ReservationsWebConfiguration configuration,
        [NoAutoProperties] HomeController controller)
    {
        // Arrange
        rootConfig.Setup(x => x["AuthType"]).Returns("employer");
        rootConfig.Setup(x => x["StubAuth"]).Returns("true");
        configuration.DashboardUrl = redirectUrl;
        controller.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
        
        var mockAuthService = new Mock<IAuthenticationService>();
        mockAuthService.Setup(x => x.SignOutAsync(It.IsAny<HttpContext>(), It.IsAny<string>(), It.IsAny<AuthenticationProperties>()))
            .Returns(Task.CompletedTask);
        
        var httpContext = new DefaultHttpContext
        {
            RequestServices = Mock.Of<IServiceProvider>(sp => 
                sp.GetService(typeof(IAuthenticationService)) == mockAuthService.Object)
        };
        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

        // Act
        await controller.SignOut();
        
        // Assert
        controller.TempData["AutoSignOut"].Should().Be(false);
        
        mockAuthService.Verify(x => x.SignOutAsync(
            It.IsAny<HttpContext>(), 
            CookieAuthenticationDefaults.AuthenticationScheme, 
            It.IsAny<AuthenticationProperties>()), Times.Once);
        mockAuthService.Verify(x => x.SignOutAsync(
            It.IsAny<HttpContext>(), 
            OpenIdConnectDefaults.AuthenticationScheme, 
            It.IsAny<AuthenticationProperties>()), Times.Never);
    }

    [Test, MoqAutoData]
    public async Task Then_It_Signs_EmployerUserOut_With_OpenIdConnect(
        string redirectUrl,
        [Frozen] Mock<IConfiguration> rootConfig,
        [Frozen] ReservationsWebConfiguration configuration,
        [NoAutoProperties] HomeController controller)
    {
        // Arrange
        rootConfig.Setup(x => x["AuthType"]).Returns("employer");
        rootConfig.Setup(x => x["StubAuth"]).Returns("false");
        configuration.DashboardUrl = redirectUrl;
        controller.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
        
        var mockAuthService = new Mock<IAuthenticationService>();
        mockAuthService.Setup(x => x.SignOutAsync(It.IsAny<HttpContext>(), It.IsAny<string>(), It.IsAny<AuthenticationProperties>()))
            .Returns(Task.CompletedTask);
        
        var httpContext = new DefaultHttpContext
        {
            RequestServices = Mock.Of<IServiceProvider>(sp => 
                sp.GetService(typeof(IAuthenticationService)) == mockAuthService.Object)
        };
        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

        // Act
        await controller.SignOut();
        
        // Assert
        controller.TempData["AutoSignOut"].Should().Be(false);
        
        mockAuthService.Verify(x => x.SignOutAsync(
            It.IsAny<HttpContext>(), 
            CookieAuthenticationDefaults.AuthenticationScheme, 
            It.IsAny<AuthenticationProperties>()), Times.Once);
        mockAuthService.Verify(x => x.SignOutAsync(
            It.IsAny<HttpContext>(), 
            OpenIdConnectDefaults.AuthenticationScheme, 
            It.IsAny<AuthenticationProperties>()), Times.Once);
    }
}