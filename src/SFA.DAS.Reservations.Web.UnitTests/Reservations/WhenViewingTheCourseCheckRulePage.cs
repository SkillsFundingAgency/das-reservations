using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.FundingRules.Queries.GetNextUnreadGlobalFundingRule;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Domain.Rules;
using SFA.DAS.Reservations.Domain.Rules.Api;
using SFA.DAS.Reservations.Web.Controllers;
using SFA.DAS.Reservations.Web.Infrastructure;
using SFA.DAS.Reservations.Web.Models;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Reservations.Web.UnitTests.Reservations
{
    public class WhenViewingTheCourseCheckRulePage
    {

        [Test, MoqAutoData]
        public async Task Then_If_Employer_Redirect_To_Funding_Notification_If_Funding_Rules_Exist(
            ReservationsRouteModel routeModel,
            string expectedUserId,
            string expectedBackUrl,
            [Frozen] Mock<IMediator> mockMediator,
            [Frozen] Mock<IExternalUrlHelper> externalUrlHelper,
            [NoAutoProperties] ReservationsController controller)
        {
            //Arrange
            routeModel.UkPrn = null;
            var expectedRule = new GlobalRule
            {
                Id = 2,
                ActiveFrom = DateTime.Now.AddDays(2)
            };
            controller.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(EmployerClaims.IdamsUserIdClaimTypeIdentifier, expectedUserId)
            }));
            var result = new GetNextUnreadGlobalFundingRuleResult { Rule = expectedRule };
            mockMediator.Setup(x => x.Send(It.Is<GetNextUnreadGlobalFundingRuleQuery>(c=>c.Id.Equals(expectedUserId)), It.IsAny<CancellationToken>()))
                .ReturnsAsync(result);
            externalUrlHelper.Setup(x => x.GenerateCohortDetailsUrl(null, routeModel.EmployerAccountId,
                routeModel.CohortReference, routeModel.CohortReference == string.Empty,
                It.IsAny<string>(), string.Empty)).Returns(expectedBackUrl);

            //act 
            var view = await controller.SelectCourseRuleCheck(routeModel) as ViewResult;

            //assert
            Assert.IsNotNull(view);
            var viewModel = view?.Model as FundingRestrictionNotificationViewModel;
            Assert.IsNotNull(viewModel);
            Assert.AreEqual(view.ViewName, "FundingRestrictionNotification");
            Assert.AreEqual(expectedRule.Id, viewModel.RuleId);
            Assert.AreEqual(RuleType.GlobalRule, viewModel.TypeOfRule);
            Assert.AreEqual(expectedRule.ActiveFrom, viewModel.RestrictionStartDate);
            Assert.AreEqual(expectedBackUrl, viewModel.BackLink);
            Assert.AreEqual(false, viewModel.IsProvider);
            Assert.AreEqual(RouteNames.EmployerSelectCourse, viewModel.RouteName);
            Assert.AreEqual(RouteNames.EmployerSaveRuleNotificationChoice, viewModel.PostRouteName);
        }

        [Test, MoqAutoData]
        public async Task Then_If_Employer_Redirect_To_Employer_Select_Course_If_No_Funding_Rules_Exist(
            ReservationsRouteModel routeModel,
            string expectedUserId,
            [Frozen] Mock<IMediator> mockMediator,
            [NoAutoProperties] ReservationsController controller)
        {
            routeModel.UkPrn = null;
            controller.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(EmployerClaims.IdamsUserIdClaimTypeIdentifier, expectedUserId)
            }));
            var result = new GetNextUnreadGlobalFundingRuleResult { Rule = null };
            mockMediator.Setup(x => x.Send(It.IsAny<GetNextUnreadGlobalFundingRuleQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(result);

            //act 
            var actual = await controller.SelectCourseRuleCheck(routeModel) as RedirectToRouteResult;

            //assert
            Assert.IsNotNull(actual);
            Assert.AreEqual(actual.RouteName, RouteNames.EmployerSelectCourse);
        }

        [Test, MoqAutoData]
        public async Task Then_If_Provider_Redirect_To_Funding_Notification_If_Funding_Rules_Exist(
            ReservationsRouteModel routeModel,
            string expectedUkprn,
            string expectedBackUrl,
            [Frozen] Mock<IMediator> mockMediator,
            [Frozen] Mock<IExternalUrlHelper> externalUrlHelper,
            [NoAutoProperties] ReservationsController controller)
        {
            //Arrange
            var expectedRule = new GlobalRule
            {
                Id = 2,
                ActiveFrom = DateTime.Now.AddDays(2)
            };
            controller.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ProviderClaims.ProviderUkprn, expectedUkprn)
            }));
            var result = new GetNextUnreadGlobalFundingRuleResult { Rule = expectedRule };
            mockMediator.Setup(x => x.Send(It.Is<GetNextUnreadGlobalFundingRuleQuery>(c=>c.Id.Equals(expectedUkprn)), It.IsAny<CancellationToken>()))
                .ReturnsAsync(result);
            externalUrlHelper.Setup(x => x.GenerateCohortDetailsUrl(routeModel.UkPrn, routeModel.EmployerAccountId,
                routeModel.CohortReference, routeModel.CohortReference == string.Empty,
                It.IsAny<string>(), string.Empty)).Returns(expectedBackUrl);

            //act 
            var view = await controller.SelectCourseRuleCheck(routeModel) as ViewResult;

            //assert
            Assert.IsNotNull(view);
            var viewModel = view?.Model as FundingRestrictionNotificationViewModel;
            Assert.IsNotNull(viewModel);
            Assert.AreEqual(view.ViewName, "FundingRestrictionNotification");
            Assert.AreEqual(expectedRule.Id, viewModel.RuleId);
            Assert.AreEqual(RuleType.GlobalRule, viewModel.TypeOfRule);
            Assert.AreEqual(expectedRule.ActiveFrom, viewModel.RestrictionStartDate);
            Assert.AreEqual(expectedBackUrl, viewModel.BackLink);
            Assert.AreEqual(true, viewModel.IsProvider);
            Assert.AreEqual(RouteNames.ProviderApprenticeshipTraining, viewModel.RouteName);
            Assert.AreEqual(RouteNames.ProviderSaveRuleNotificationChoice, viewModel.PostRouteName);
        }

        [Test, MoqAutoData]
        public async Task Then_If_Provider_Redirect_To_Provider_Select_Course_If_No_Funding_Rules_Exist(
            ReservationsRouteModel routeModel,
            string expectedUkprn,
            [Frozen] Mock<IMediator> mockMediator,
            [NoAutoProperties] ReservationsController controller)
        {
            controller.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ProviderClaims.ProviderUkprn, expectedUkprn)
            }));
            var result = new GetNextUnreadGlobalFundingRuleResult { Rule = null };
            mockMediator.Setup(x => x.Send(It.IsAny<GetNextUnreadGlobalFundingRuleQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(result);

            //act 
            var actual = await controller.SelectCourseRuleCheck(routeModel) as RedirectToRouteResult;

            //assert
            Assert.IsNotNull(actual);
            Assert.AreEqual(actual.RouteName, RouteNames.ProviderApprenticeshipTraining);
        }
    }
}
