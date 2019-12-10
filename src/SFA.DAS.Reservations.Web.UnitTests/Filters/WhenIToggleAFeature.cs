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
        public void Then_If_Toggled_Off_Request_Is_Redirected(
            [Frozen] Mock<IConfiguration> configuration,
            Mock<ActionExecutingContext> context)
        {
            //Assign
            IActionResult result = null;
            
            configuration.SetupGet(c => c["FeatureToggleOn"]).Returns("False");
            context.SetupSet(c => c.Result = It.IsAny<IActionResult>())
                .Callback<IActionResult>(r =>
                {
                    result = r;
                });

            var filter = new FeatureToggleActionFilter(configuration.Object);

            ////Act
            filter.OnActionExecuting(context.Object);

            var redirect = result as RedirectToActionResult;

            //Assert
            Assert.IsNotNull(redirect);
            Assert.AreEqual("Home", redirect.ControllerName);
            Assert.AreEqual("FeatureNotAvailable", redirect.ActionName);

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