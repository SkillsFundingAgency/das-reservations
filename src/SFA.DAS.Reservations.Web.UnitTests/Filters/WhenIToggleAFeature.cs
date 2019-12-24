using System.Collections.Generic;
using AutoFixture.NUnit3;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Web.Filters;
using SFA.DAS.Reservations.Web.Infrastructure;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Reservations.Web.UnitTests.Filters
{
    public class WhenIToggleAFeature
    {
        [Test, MoqAutoData]
        public void Then_Toggle_Is_Checked(
            [Frozen] Mock<IConfiguration> configuration, 
            ActionExecutedContext context)
        {
            //Assign
            configuration.Setup(c => c["FeatureToggleOn"]).Returns("True");

            var filter = new FeatureToggleActionFilter(configuration.Object);

            //Act
            filter.OnActionExecuted(context);

            //Assert
            configuration.Verify(c => c["FeatureToggleOn"], Times.Once);

        }

        [Test, MoqAutoData]
        public void Then_If_Toggled_Off_For_An_Employer_The_Request_Is_Redirected(
            string accountId,
            HttpContext httpContext,
            ActionDescriptor actionDescriptor,
            IList<IFilterMetadata> filters,
            Dictionary<string, object> actionArguments,
            object controller,
            [Frozen] Mock<IConfiguration> configuration)
        {
            // Arrange
            configuration.SetupGet(c => c["FeatureToggleOn"]).Returns("False");
            var routeData = new RouteData();
            routeData.Values.Add("employerAccountId", accountId);
            
            
            var actionContext = new ActionContext(httpContext, routeData, actionDescriptor);
            var context =
                new ActionExecutingContext(actionContext, filters, actionArguments, controller)
                {
                    Result = new ContentResult()
                };

            var filter = new FeatureToggleActionFilter(configuration.Object);

            // Act
            filter.OnActionExecuting(context);

            var redirect = context.Result as RedirectToRouteResult;

            // Assert
            Assert.IsNotNull(redirect);
            Assert.AreEqual(RouteNames.EmployerFeatureNotAvailable, redirect.RouteName);
            Assert.AreEqual(accountId, redirect.RouteValues["employerAccountId"]);

        }
        [Test, MoqAutoData]
        public void Then_If_Toggled_Off_For_A_Provider_The_Request_Is_Redirected(
            string ukprn,
            HttpContext httpContext,
            ActionDescriptor actionDescriptor,
            IList<IFilterMetadata> filters,
            Dictionary<string, object> actionArguments,
            object controller,
            [Frozen] Mock<IConfiguration> configuration)
        {
            // Arrange
            configuration.SetupGet(c => c["FeatureToggleOn"]).Returns("False");
            var routeData = new RouteData();
            routeData.Values.Add("ukPrn", ukprn);
            
            
            var actionContext = new ActionContext(httpContext, routeData, actionDescriptor);
            var context =
                new ActionExecutingContext(actionContext, filters, actionArguments, controller)
                {
                    Result = new ContentResult()
                };

            var filter = new FeatureToggleActionFilter(configuration.Object);

            // Act
            filter.OnActionExecuting(context);

            var redirect = context.Result as RedirectToRouteResult;

            // Assert
            Assert.IsNotNull(redirect);
            Assert.AreEqual(RouteNames.ProviderFeatureNotAvailable, redirect.RouteName);
            Assert.AreEqual(ukprn, redirect.RouteValues["ukPrn"]);

        }

        [Test, MoqAutoData]
        public void Then_If_Toggled_Off_And_No_Employer_Or_Provider_Route_Data_An_Unauhtorised_Page_Is_Shown(
            HttpContext httpContext,
            ActionDescriptor actionDescriptor,
            IList<IFilterMetadata> filters,
            Dictionary<string, object> actionArguments,
            object controller,
            [Frozen] Mock<IConfiguration> configuration)
        {
            // Arrange
            configuration.SetupGet(c => c["FeatureToggleOn"]).Returns("False");
            var routeData = new RouteData();
            var actionContext = new ActionContext(httpContext, routeData, actionDescriptor);
            var context =
                new ActionExecutingContext(actionContext, filters, actionArguments, controller)
                {
                    Result = new ContentResult()
                };

            var filter = new FeatureToggleActionFilter(configuration.Object);

            // Act
            filter.OnActionExecuting(context);

            var redirect = context.Result as RedirectToRouteResult;

            // Assert
            Assert.IsNotNull(redirect);
            Assert.AreEqual(RouteNames.Error403, redirect.RouteName);
            
        }

        [Test, MoqAutoData]
        public void Then_If_Toggled_On_Request_Is_Handled(
            [Frozen] Mock<IConfiguration> configuration,
            Mock<ActionExecutingContext> context)
        {
            //Assign
            configuration.Setup(c => c["FeatureToggleOn"]).Returns("True");

            var filter = new FeatureToggleActionFilter(configuration.Object);

            //Check that contect Result is only called once beforehand
            context.VerifySet(c => c.Result = It.IsAny<IActionResult>(), Times.Once);

            //Act
            filter.OnActionExecuting(context.Object);

            //Assert
            context.VerifySet(c => c.Result = It.IsAny<IActionResult>(), Times.Once);
        }

        [Test, MoqAutoData]
        public void Then_If_Toggled_Off_Request_Will_Not_Try_To_Redirect_If_Already_Redirected(
            [Frozen] Mock<IConfiguration> configuration,
            HttpContext httpContext,
            ActionDescriptor actionDescriptor,
            IList<IFilterMetadata> filters,
            Dictionary<string, object> actionArguments,
            object controller)
        {
            //Assign
            var routeData = new RouteData();
            routeData.Values.Add("controller", "home");
            routeData.Values.Add("action", "featurenotavailable");
            
            
            var actionContext = new ActionContext(httpContext, routeData, actionDescriptor);
            var context =
                new ActionExecutingContext(actionContext, filters, actionArguments, controller)
                {
                    Result = new ContentResult()
                };

            configuration.SetupGet(c => c["FeatureToggleOn"]).Returns("False");

            var filter = new FeatureToggleActionFilter(configuration.Object);

            ////Act
            filter.OnActionExecuting(context);

            var redirect =  context.Result as RedirectToActionResult;

            //Assert
            Assert.IsNull(redirect);
        }
    }
}