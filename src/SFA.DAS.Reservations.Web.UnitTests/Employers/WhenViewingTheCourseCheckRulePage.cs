using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.FundingRules.Queries.GetNextUnreadGlobalFundingRule;
using SFA.DAS.Reservations.Domain.Rules;
using SFA.DAS.Reservations.Domain.Rules.Api;
using SFA.DAS.Reservations.Infrastructure.Configuration;
using SFA.DAS.Reservations.Web.Controllers;
using SFA.DAS.Reservations.Web.Infrastructure;
using SFA.DAS.Reservations.Web.Models;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Reservations.Web.UnitTests.Employers
{
    public class WhenViewingTheCourseCheckRulePage
    {

        [Test, MoqAutoData]
        public async Task Then_Redirect_To_Funding_Notification_If_Funding_Rules_Exist(
            ReservationsRouteModel routeModel,
            string expectedUserId,
            [Frozen] Mock<IOptions<ReservationsWebConfiguration>> config,
            [Frozen] Mock<IMediator> mockMediator,
            EmployerReservationsController controller)
        {
            //Arrange
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
            mockMediator.Setup(x => x.Send(It.IsAny<GetNextUnreadGlobalFundingRuleQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(result);

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
            Assert.AreEqual(config.Object.Value.EmployerDashboardUrl, viewModel.BackLink);
            Assert.AreEqual(RouteNames.EmployerSelectCourse, viewModel.RouteName);
        }

        [Test, MoqAutoData]
        public async Task Then_Redirect_To_Select_Course_If_No_Funding_Rules_Exist(
            ReservationsRouteModel routeModel,
            string expectedUserId,
            [Frozen] Mock<IMediator> mockMediator,
            EmployerReservationsController controller)
        {
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
    }
}
