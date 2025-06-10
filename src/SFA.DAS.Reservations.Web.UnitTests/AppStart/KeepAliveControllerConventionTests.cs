using System.Reflection;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Web.AppStart;
using SFA.DAS.Reservations.Web.Controllers;

namespace SFA.DAS.Reservations.Web.UnitTests.AppStart;

public class KeepAliveControllerConventionTests
{
    private Mock<IConfiguration> _configuration;
    private ApplicationModel _applicationModel;
    private KeepAliveControllerConvention _convention;

    [SetUp]
    public void Setup()
    {
        _configuration = new Mock<IConfiguration>();
        _applicationModel = new ApplicationModel();
        _convention = new KeepAliveControllerConvention(_configuration.Object);
    }

    [Test]
    public void When_EmployerAuth_Then_Removes_DfESignIn_KeepAlive_Controller()
    {
        // Arrange
        _configuration.Setup(x => x["AuthType"]).Returns("employer");
        
        var dfESignInController = new ControllerModel(
            typeof(SFA.DAS.DfESignIn.Auth.Controllers.SessionKeepAliveController).GetTypeInfo(),
            [new AttributeRouteModel { Template = "service/keepalive" }]);
        dfESignInController.ControllerName = "SessionKeepAlive";
        
        var govUkController = new ControllerModel(
            typeof(SFA.DAS.GovUK.Auth.Controllers.SessionKeepAliveController).GetTypeInfo(),
            [new AttributeRouteModel { Template = "service/keepalive" }]);
        govUkController.ControllerName = "SessionKeepAlive";

        _applicationModel.Controllers.Add(dfESignInController);
        _applicationModel.Controllers.Add(govUkController);

        // Act
        _convention.Apply(_applicationModel);

        // Assert
        _applicationModel.Controllers.Should().HaveCount(1);
        _applicationModel.Controllers[0].ControllerType.Namespace.Should().Contain("SFA.DAS.GovUK.Auth");
    }

    [Test]
    public void When_ProviderAuth_Then_Removes_GovUK_KeepAlive_Controller()
    {
        // Arrange
        _configuration.Setup(x => x["AuthType"]).Returns("provider");
        
        var dfESignInController = new ControllerModel(
            typeof(SFA.DAS.DfESignIn.Auth.Controllers.SessionKeepAliveController).GetTypeInfo(),
            [new AttributeRouteModel { Template = "service/keepalive" }]);
        dfESignInController.ControllerName = "SessionKeepAlive";
        
        var govUkController = new ControllerModel(
            typeof(SFA.DAS.GovUK.Auth.Controllers.SessionKeepAliveController).GetTypeInfo(),
            [new AttributeRouteModel { Template = "service/keepalive" }]);
        govUkController.ControllerName = "SessionKeepAlive";

        _applicationModel.Controllers.Add(dfESignInController);
        _applicationModel.Controllers.Add(govUkController);

        // Act
        _convention.Apply(_applicationModel);

        // Assert
        _applicationModel.Controllers.Should().HaveCount(1);
        _applicationModel.Controllers[0].ControllerType.Namespace.Should().Contain("SFA.DAS.DfESignIn.Auth");
    }

    [Test]
    public void When_Other_Controllers_Present_Then_Does_Not_Remove_Them()
    {
        // Arrange
        _configuration.Setup(x => x["AuthType"]).Returns("employer");
        
        var dfESignInController = new ControllerModel(
            typeof(SFA.DAS.DfESignIn.Auth.Controllers.SessionKeepAliveController).GetTypeInfo(),
            [new AttributeRouteModel { Template = "service/keepalive" }]);
        dfESignInController.ControllerName = "SessionKeepAlive";
        
        var otherController = new ControllerModel(
            typeof(HomeController).GetTypeInfo(),
            [new AttributeRouteModel { Template = "home" }]);
        otherController.ControllerName = "Home";

        _applicationModel.Controllers.Add(dfESignInController);
        _applicationModel.Controllers.Add(otherController);

        // Act
        _convention.Apply(_applicationModel);

        // Assert
        _applicationModel.Controllers.Should().HaveCount(1);
        _applicationModel.Controllers[0].ControllerName.Should().Be("Home");
    }
} 