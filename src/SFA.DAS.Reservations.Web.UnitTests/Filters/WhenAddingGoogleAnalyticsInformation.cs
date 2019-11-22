using System;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
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
            GoogleAnalyticsFilter filter)
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
    }
}