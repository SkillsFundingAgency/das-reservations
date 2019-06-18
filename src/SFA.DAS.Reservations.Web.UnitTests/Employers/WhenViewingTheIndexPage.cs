using System;
using System.Security.Claims;
using System.Threading;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Web.Controllers;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Microsoft.Extensions.Options;
using SFA.DAS.Reservations.Application.FundingRules.Queries.GetNextUnreadGlobalFundingRule;
using SFA.DAS.Reservations.Domain.Rules;
using SFA.DAS.Reservations.Domain.Rules.Api;
using SFA.DAS.Reservations.Infrastructure.Configuration;
using SFA.DAS.Reservations.Web.Infrastructure;
using SFA.DAS.Reservations.Web.Models;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Reservations.Web.UnitTests.Employers
{
    public class WhenViewingTheIndexPage
    {
       
        [Test, MoqAutoData]
        public async Task ThenChecksIfRelatedUnreadRulesExists(
            string expectedUserId,
            [Frozen] Mock<IMediator> mockMediator,
            EmployerReservationsController controller)
        {
            //Arrange
            controller.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(EmployerClaims.IdamsUserIdClaimTypeIdentifier, expectedUserId)
            }));
            mockMediator.Setup(x =>
                    x.Send(It.IsAny<GetNextUnreadGlobalFundingRuleQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetNextUnreadGlobalFundingRuleResult());

            //act 
            await controller.Index();

            //assert
            mockMediator.Verify(m => m.Send(It.Is<GetNextUnreadGlobalFundingRuleQuery>(
                q => q.Id.Equals(expectedUserId, StringComparison.CurrentCultureIgnoreCase)),It.IsAny<CancellationToken>()),Times.Once);
        }

        [Test, MoqAutoData]
        public async Task ThenRedirectToStartIfNoFundingRulesExist(
            string expectedUserId,
            [Frozen] Mock<IMediator> mockMediator,
            EmployerReservationsController controller)
        {
            //arrange
            controller.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(EmployerClaims.IdamsUserIdClaimTypeIdentifier, expectedUserId)
            }));
            mockMediator.Setup(x => x.Send(It.IsAny<GetNextUnreadGlobalFundingRuleQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((GetNextUnreadGlobalFundingRuleResult) null);

            //act 
            var redirect = await controller.Index() as RedirectToActionResult;

            //assert
            Assert.IsNotNull(redirect);
            Assert.AreEqual(redirect.ActionName, "Start");
        }

        [Test, MoqAutoData]
        public async Task ThenRedirectToFundingPausedIfFundingRulesExist(
            string expectedUserId,
            [Frozen] Mock<IMediator> mockMediator,
            [Frozen] Mock<IOptions<ReservationsWebConfiguration>> config,
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
            var view = await controller.Index() as ViewResult;

            //assert
            Assert.IsNotNull(view);
            var viewModel = view?.Model as FundingRestrictionNotificationViewModel;
            Assert.IsNotNull(viewModel);
            Assert.AreEqual(view.ViewName, "FundingRestrictionNotification");
            Assert.AreEqual(expectedRule.Id, viewModel.RuleId);
            Assert.AreEqual(RuleType.GlobalRule, viewModel.TypeOfRule);
            Assert.AreEqual(expectedRule.ActiveFrom, viewModel.RestrictionStartDate);
            Assert.AreEqual(config.Object.Value.EmployerDashboardUrl, viewModel.BackLink);
        }

        [Test, MoqAutoData]
        public async Task ThenRedirectToStartIfNoIdFoundOnNextGlobalFundingRule(
            string expectedUserId,
            [Frozen] Mock<IMediator> mockMediator,
            EmployerReservationsController controller)
        {
            //arrange
            controller.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(EmployerClaims.IdamsUserIdClaimTypeIdentifier, expectedUserId)
            }));
            mockMediator.Setup(x => x.Send(It.IsAny<GetNextUnreadGlobalFundingRuleQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetNextUnreadGlobalFundingRuleResult {Rule = new GlobalRule{ActiveFrom = DateTime.Now}});

            //act 
            var redirect = await controller.Index() as RedirectToActionResult;

            //assert
            Assert.IsNotNull(redirect);
            Assert.AreEqual(redirect.ActionName, "Start");
        }

        [Test, MoqAutoData]
        public async Task ThenRedirectToStartIfNoActiveFromDateFoundOnNextGlobalFundingRule(
            string expectedUserId,
            [Frozen] Mock<IMediator> mockMediator,
            EmployerReservationsController controller)
        {
            //arrange
            controller.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(EmployerClaims.IdamsUserIdClaimTypeIdentifier, expectedUserId)
            }));
            mockMediator.Setup(x => x.Send(It.IsAny<GetNextUnreadGlobalFundingRuleQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetNextUnreadGlobalFundingRuleResult {Rule = new GlobalRule{Id = 2}});

            //act 
            var redirect = await controller.Index() as RedirectToActionResult;

            //assert
            Assert.IsNotNull(redirect);
            Assert.AreEqual(redirect.ActionName, "Start");
        }
    }
}
