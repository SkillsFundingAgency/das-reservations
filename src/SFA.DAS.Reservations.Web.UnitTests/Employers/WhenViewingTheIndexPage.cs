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
using Microsoft.AspNetCore.Mvc.Routing;
using SFA.DAS.Reservations.Application.FundingRules.Queries.GetNextUnreadGlobalFundingRule;
using SFA.DAS.Reservations.Domain.Rules;
using SFA.DAS.Reservations.Domain.Rules.Api;
using SFA.DAS.Reservations.Web.Infrastructure;
using SFA.DAS.Reservations.Web.Models;
using SFA.DAS.Testing.AutoFixture;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace SFA.DAS.Reservations.Web.UnitTests.Employers
{
    public class WhenViewingTheIndexPage
    {
       
        [Test, MoqAutoData]
        public async Task ThenChecksIfRelatedUnreadRulesExists(
            string expectedUserId,
            [Frozen] Mock<IMediator> mockMediator,
            ReservationsRouteModel routeModel,
            IUrlHelper urlHelper,
            [NoAutoProperties] EmployerReservationsController controller)
        {
            //Arrange
            var user = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim> { new Claim(EmployerClaims.IdamsUserIdClaimTypeIdentifier, expectedUserId) }));
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
                { User = user }
            };
            controller.Url = urlHelper;
          
            mockMediator.Setup(x =>
                    x.Send(It.IsAny<GetNextUnreadGlobalFundingRuleQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetNextUnreadGlobalFundingRuleResult());

            //act 
            await controller.Index(routeModel);

            //assert
            mockMediator.Verify(m => m.Send(It.Is<GetNextUnreadGlobalFundingRuleQuery>(
                q => q.Id.Equals(expectedUserId, StringComparison.CurrentCultureIgnoreCase)),It.IsAny<CancellationToken>()),Times.Once);
        }

        [Test, MoqAutoData]
        public async Task ThenRedirectToStartIfNoFundingRulesExist(
            string expectedUserId,
            [Frozen] Mock<IMediator> mockMediator,
            ReservationsRouteModel routeModel,
            [NoAutoProperties] EmployerReservationsController controller)
        {
            //arrange
            var user = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim> { new Claim(EmployerClaims.IdamsUserIdClaimTypeIdentifier, expectedUserId) }));
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
                { User = user }
            };
            controller.Url = Mock.Of<IUrlHelper>();
            mockMediator.Setup(x => x.Send(It.IsAny<GetNextUnreadGlobalFundingRuleQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((GetNextUnreadGlobalFundingRuleResult) null);

            //act 
            var redirect = await controller.Index(routeModel) as RedirectToRouteResult;

            //assert
            Assert.IsNotNull(redirect);
            Assert.AreEqual(redirect.RouteName, RouteNames.EmployerStart);
        }

        [Test, MoqAutoData]
        public async Task ThenRedirectToFundingNotificationIfFundingRulesExist(
            string expectedUserId,
            [Frozen] Mock<IMediator> mockMediator,
            [Frozen] Mock<IUrlHelper> urlHelper,
            string expectedBackUrl,
            ReservationsRouteModel routeModel,
            [NoAutoProperties] EmployerReservationsController controller)
        {
            //Arrange
            urlHelper.Setup(h => h.RouteUrl(It.Is<UrlRouteContext>(c =>
                    c.RouteName.Equals(RouteNames.EmployerManage))))
                .Returns(expectedBackUrl);
            var expectedRule = new GlobalRule
            {
                Id = 2,
                ActiveFrom = DateTime.Now.AddDays(2)
            };
            var user = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim> { new Claim(EmployerClaims.IdamsUserIdClaimTypeIdentifier, expectedUserId) }));
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
                { User = user }
            };
            controller.Url = urlHelper.Object;
            var result = new GetNextUnreadGlobalFundingRuleResult { Rule = expectedRule };
            mockMediator.Setup(x => x.Send(It.IsAny<GetNextUnreadGlobalFundingRuleQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(result);

            //act 
            var view = await controller.Index(routeModel) as ViewResult;

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
            Assert.AreEqual(RouteNames.EmployerStart, viewModel.RouteName);
            Assert.AreEqual(RouteNames.EmployerSaveRuleNotificationChoiceNoReservation, viewModel.PostRouteName);
        }

        [Test, MoqAutoData]
        public async Task ThenRedirectToStartIfNoIdFoundOnNextGlobalFundingRule(
            string expectedUserId,
            [Frozen] Mock<IMediator> mockMediator,
            ReservationsRouteModel routeModel,
            [NoAutoProperties] EmployerReservationsController controller)
        {
            //arrange
            var user = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim> { new Claim(EmployerClaims.IdamsUserIdClaimTypeIdentifier, expectedUserId) }));
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
                { User = user }
            };
            controller.Url = Mock.Of<IUrlHelper>();
            mockMediator.Setup(x => x.Send(It.IsAny<GetNextUnreadGlobalFundingRuleQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetNextUnreadGlobalFundingRuleResult {Rule = new GlobalRule{ActiveFrom = DateTime.Now}});

            //act 
            var redirect = await controller.Index(routeModel) as RedirectToRouteResult;

            //assert
            Assert.IsNotNull(redirect);
            Assert.AreEqual(redirect.RouteName, RouteNames.EmployerStart);
        }

        [Test, MoqAutoData]
        public async Task ThenRedirectToStartIfNoActiveFromDateFoundOnNextGlobalFundingRule(
            string expectedUserId,
            [Frozen] Mock<IMediator> mockMediator,
            ReservationsRouteModel routeModel,
            [NoAutoProperties] EmployerReservationsController controller)
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim> { new Claim(EmployerClaims.IdamsUserIdClaimTypeIdentifier, expectedUserId) }));
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
                { User = user }
            };
            controller.Url = Mock.Of<IUrlHelper>();
            mockMediator.Setup(x => x.Send(It.IsAny<GetNextUnreadGlobalFundingRuleQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetNextUnreadGlobalFundingRuleResult {Rule = new GlobalRule{Id = 2}});

            //act 
            var redirect = await controller.Index(routeModel) as RedirectToRouteResult;

            //assert
            Assert.IsNotNull(redirect);
            Assert.AreEqual(redirect.RouteName, RouteNames.EmployerStart);
        }
    }
}
