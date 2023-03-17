using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.FundingRules.Commands.MarkRuleAsRead;
using SFA.DAS.Reservations.Web.Controllers;
using SFA.DAS.Reservations.Web.Infrastructure;
using SFA.DAS.Reservations.Web.Models;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Reservations.Web.UnitTests.Reservations
{
    public class WhenSavingRuleNotificationChoice
    {
        
        [Test, MoqAutoData]
        public async Task Then_If_Provider_Reads_Ukprn_Claim_And_Sends_Command_To_Mark_Rule_As_Read_And_Redirects_To_Route_In_Model(
            FundingRestrictionNotificationViewModel viewModel,
            ReservationsRouteModel routeModel,
            string expectedUkprn,
            uint? ukprn,
            [Frozen] Mock<IMediator> mockMediator,
            [NoAutoProperties] ReservationsController controller)
        {
            //arrange
            viewModel.RouteName = RouteNames.ProviderApprenticeshipTraining;
            routeModel.UkPrn = ukprn;
            viewModel.MarkRuleAsRead = true;

            var claim = new Claim(ProviderClaims.ProviderUkprn, expectedUkprn);
            var user = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim> { claim }));
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
                { User = user }
            };

            //act
            var actual = await controller.SaveRuleNotificationChoice(routeModel, viewModel) as RedirectToRouteResult;

            //assert
            mockMediator.Verify(m => m.Send(It.Is<MarkRuleAsReadCommand>(c => 
                c.Id.Equals(expectedUkprn) &&
                c.RuleId.Equals(viewModel.RuleId) &&
                c.TypeOfRule.Equals(viewModel.TypeOfRule)), It.IsAny<CancellationToken>()));
            Assert.IsNotNull(actual);
            Assert.AreEqual(viewModel.RouteName, actual.RouteName);

        }

        [Test, MoqAutoData]
        public async Task Then_If_Employer_Reads_UserId_Claim_And_Sends_Command_To_Mark_Rule_As_Read_And_Redirects_To_Route_In_Model(
            FundingRestrictionNotificationViewModel viewModel,
            ReservationsRouteModel routeModel,
            string expectedUserId,
            [Frozen] Mock<IMediator> mockMediator,
            [NoAutoProperties] ReservationsController controller)
        {
            //arrange
            viewModel.RouteName = RouteNames.ProviderApprenticeshipTraining;
            routeModel.UkPrn = null;
            viewModel.MarkRuleAsRead = true;

            var claim = new Claim(EmployerClaims.IdamsUserIdClaimTypeIdentifier, expectedUserId);
            var user = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim> { claim }));
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
                { User = user }
            };

            //act
            var actual = await controller.SaveRuleNotificationChoice(routeModel, viewModel) as RedirectToRouteResult;

            //assert
            mockMediator.Verify(m => m.Send(It.Is<MarkRuleAsReadCommand>(c =>
                c.Id.Equals(expectedUserId) &&
                c.RuleId.Equals(viewModel.RuleId) &&
                c.TypeOfRule.Equals(viewModel.TypeOfRule)), It.IsAny<CancellationToken>()));
            Assert.IsNotNull(actual);
            Assert.AreEqual(viewModel.RouteName, actual.RouteName);

        }

        [Test, MoqAutoData]
        public async Task ThenDoesNotSendsCommandIfNotMarkedAsReadAnd_Redirects_To_Route_In_Model(
            FundingRestrictionNotificationViewModel viewModel,
            ReservationsRouteModel routeModel,
            string expectedUserId,
            [Frozen] Mock<IMediator> mockMediator,
            [NoAutoProperties] ReservationsController controller)
        {
            //Arrange
            viewModel.MarkRuleAsRead = false;

            //act
            var actual = await controller.SaveRuleNotificationChoice(routeModel, viewModel) as RedirectToRouteResult;

            //assert
            mockMediator.Verify(m => m.Send(It.Is<MarkRuleAsReadCommand>(c =>
                c.Id.Equals(expectedUserId) &&
                c.RuleId.Equals(viewModel.RuleId) &&
                c.TypeOfRule.Equals(viewModel.TypeOfRule)), It.IsAny<CancellationToken>()), Times.Never);
            Assert.IsNotNull(actual);
            Assert.AreEqual(viewModel.RouteName, actual.RouteName);
        }

    }
}
