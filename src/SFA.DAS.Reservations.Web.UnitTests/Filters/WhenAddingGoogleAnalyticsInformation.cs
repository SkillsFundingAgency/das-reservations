using System;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Azure.Documents.SystemFunctions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Web.AppStart;
using SFA.DAS.Reservations.Web.Filters;
using SFA.DAS.Reservations.Web.Infrastructure;
using SFA.DAS.Reservations.Web.Models;
using SFA.DAS.Reservations.Web.UnitTests.Customisations;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Reservations.Web.UnitTests.Filters
{
    public class WhenAddingGoogleAnalyticsInformation
    {
        [Test, MoqAutoData]
        public async Task Then_If_Provider_Adds_The_ProviderId_To_The_ViewBag_Data(
            uint ukprn,
            [Frozen] ServiceParameters serviceParameters,
            [ArrangeActionContext] ActionExecutingContext context,
            [Frozen] Mock<ActionExecutionDelegate> nextMethod,
            GoogleAnalyticsFilter filter)
        {
            //Arrange
            context.RouteData.Values.Add("ukPrn", ukprn);
            serviceParameters.AuthenticationType = AuthenticationType.Provider;
            
            //Act
            await filter.OnActionExecutionAsync(context, nextMethod.Object);

            //Assert
            var actualController = context.Controller as Controller;
            Assert.IsNotNull(actualController);
            var viewBagData = actualController.ViewBag.GaData as GaData;
            Assert.IsNotNull(viewBagData);
            Assert.AreEqual(ukprn.ToString(), viewBagData.UkPrn);
        }
        
        [Test, MoqAutoData]
        public async Task Then_If_Employer_Adds_The_AccountId_And_UserId_To_The_ViewBag_Data(
            Guid userId,
            long accountId,
            [Frozen] ServiceParameters serviceParameters,
            [ArrangeActionContext] ActionExecutingContext context,
            [Frozen] Mock<ActionExecutionDelegate> nextMethod,
            [NoAutoProperties] GoogleAnalyticsFilter filter)
        {
            //Arrange
            var claim = new Claim(EmployerClaims.IdamsUserIdClaimTypeIdentifier, userId.ToString());
            context.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[] {claim}));
            context.RouteData.Values.Add("employerAccountId", accountId);
            serviceParameters.AuthenticationType = AuthenticationType.Employer;
            
            //Act
            await filter.OnActionExecutionAsync(context, nextMethod.Object);

            //Assert
            var actualController = context.Controller as Controller;
            Assert.IsNotNull(actualController);
            var viewBagData = actualController.ViewBag.GaData as GaData;
            Assert.IsNotNull(viewBagData);
            Assert.AreEqual(accountId.ToString(), viewBagData.Acc);
            Assert.AreEqual(userId.ToString(), viewBagData.UserId);
        }
        
        
        [Test, MoqAutoData]
        public async Task Then_If_User_Is_Not_Logged_In_Then_Empty_ViewBag_Data_Is_Returned(
            long accountId,
            [Frozen] ServiceParameters serviceParameters,
            [ArrangeActionContext] ActionExecutingContext context,
            [Frozen] Mock<ActionExecutionDelegate> nextMethod,
            GoogleAnalyticsFilter filter)
        {
            //Arrange
            context.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity());
            serviceParameters.AuthenticationType = AuthenticationType.Employer;
            
            //Act
            await filter.OnActionExecutionAsync(context, nextMethod.Object);

            //Assert
            var actualController = context.Controller as Controller;
            Assert.IsNotNull(actualController);
            var viewBagData = actualController.ViewBag.GaData as GaData;
            Assert.IsNotNull(viewBagData);
            Assert.IsNull(viewBagData.Acc);
            Assert.IsNull(viewBagData.UserId);
        }

        [Test, MoqAutoData]
        public async Task Then_If_The_Controller_Is_Not_A_Controller_No_Data_Is_Added_To_ViewBag(
            Guid userId,
            long accountId,
            [Frozen] ServiceParameters serviceParameters,
            [ArrangeActionContext] ActionExecutingContext context,
            [Frozen] Mock<ActionExecutionDelegate> nextMethod,
            GoogleAnalyticsFilter filter)
        {
            //Arrange
            var claim = new Claim(EmployerClaims.IdamsUserIdClaimTypeIdentifier, userId.ToString());
            context.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { claim }));
            context.RouteData.Values.Add("employerAccountId", accountId);
            serviceParameters.AuthenticationType = AuthenticationType.Employer;

            var contextWithoutController = new ActionExecutingContext(
                new ActionContext(context.HttpContext, context.RouteData, context.ActionDescriptor), 
                context.Filters,
                context.ActionArguments,
                "");

            //Act
            await filter.OnActionExecutionAsync(contextWithoutController, nextMethod.Object);

            //Assert
            Assert.DoesNotThrowAsync(() => filter.OnActionExecutionAsync(contextWithoutController, nextMethod.Object));
        }
    }
}