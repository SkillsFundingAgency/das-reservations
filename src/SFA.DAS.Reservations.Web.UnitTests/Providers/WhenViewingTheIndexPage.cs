using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoMoq;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.FundingRules.Queries.GetNextUnreadGlobalFundingRule;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Domain.Rules;
using SFA.DAS.Reservations.Domain.Rules.Api;
using SFA.DAS.Reservations.Infrastructure.Configuration;
using SFA.DAS.Reservations.Web.Controllers;
using SFA.DAS.Reservations.Web.Infrastructure;
using SFA.DAS.Reservations.Web.Models;

namespace SFA.DAS.Reservations.Web.UnitTests.Providers
{
    public class WhenViewingTheIndexPage
    {
        private ProviderReservationsController _controller;
        private Mock<IMediator> _mockMediator;
        private GlobalRule _expectedRule;
        private ReservationsWebConfiguration _config;
        private string ExpectedUkPrn = "123";
        private string ExpectedDashboardUrl = "https://dashboard/account";
        private Mock<IExternalUrlHelper> _externalUrlHelper;

        [SetUp]
        public void Arrange()
        {
            var fixture = new Fixture().Customize(new AutoMoqCustomization {ConfigureMembers = true});

            _expectedRule = new GlobalRule {Id = 2, ActiveFrom = DateTime.Now.AddDays(2)};

           var result = new GetNextUnreadGlobalFundingRuleResult {Rule = _expectedRule};

            _mockMediator = fixture.Freeze<Mock<IMediator>>();
            _config = new ReservationsWebConfiguration
            {
                DashboardUrl = "test.com/test"
            };

            var options = fixture.Freeze<Mock<IOptions<ReservationsWebConfiguration>>>();

            options.Setup(o => o.Value).Returns(_config);
            
            _mockMediator.Setup(x => x.Send(It.IsAny<GetNextUnreadGlobalFundingRuleQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(result);

            _externalUrlHelper = fixture.Freeze<Mock<IExternalUrlHelper>>();
            _externalUrlHelper.Setup(x => x.GenerateDashboardUrl(null)).Returns(ExpectedDashboardUrl);

            
            _controller = fixture.Create<ProviderReservationsController>();
            var claim = new Claim(ProviderClaims.ProviderUkprn, ExpectedUkPrn);
            _controller.ControllerContext.HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[] {claim}))
            };
        }

        [Test]
        public async Task ThenRedirectToStartIfNoFundingRulesExist()
        {
            //arrange
            _mockMediator.Setup(x => x.Send(It.IsAny<GetNextUnreadGlobalFundingRuleQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((GetNextUnreadGlobalFundingRuleResult) null);
            var routeModel = new ReservationsRouteModel
            {
                UkPrn = 1,
                IsFromManage = true
            };

            //act 
            var redirect = await _controller.Index(routeModel) as RedirectToRouteResult;

            //assert
            Assert.IsNotNull(redirect);
            Assert.AreEqual(redirect.RouteName, RouteNames.ProviderStart);
            Assert.AreEqual(true, redirect.RouteValues["isFromManage"]);
            Assert.AreEqual(1, redirect.RouteValues["ukPrn"]);
        }

        [Test]
        public async Task ThenRedirectToFundingPausedIfFundingRulesExist()
        {
            //Arrange
            var routeModel = new ReservationsRouteModel
            {
                UkPrn = 1,
                IsFromManage = true
            };

            //act 
            var view = await _controller.Index(routeModel) as ViewResult;

            var viewModel = view?.Model as FundingRestrictionNotificationViewModel;

            //assert
            Assert.IsNotNull(view);
            Assert.IsNotNull(viewModel);
            Assert.AreEqual(view.ViewName, "FundingRestrictionNotification");
            Assert.AreEqual(_expectedRule.Id, viewModel.RuleId);
            Assert.AreEqual(RuleType.GlobalRule, viewModel.TypeOfRule);
            Assert.AreEqual(_expectedRule.ActiveFrom, viewModel.RestrictionStartDate);
            Assert.AreEqual(ExpectedDashboardUrl, viewModel.BackLink);
        }

        [Test]
        public async Task ThenRedirectToStartIfNoIdFoundOnNextGlobalFundingRule()
        {
            //Arrange
            var routeModel = new ReservationsRouteModel
            {
                UkPrn = 1,
                IsFromManage = true
            };
            _mockMediator.Setup(x => x.Send(It.IsAny<GetNextUnreadGlobalFundingRuleQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetNextUnreadGlobalFundingRuleResult {Rule = new GlobalRule{ActiveFrom = DateTime.Now}});

            //act 
            var redirect = await _controller.Index(routeModel) as RedirectToRouteResult;

            //assert
            Assert.IsNotNull(redirect);
            Assert.AreEqual(redirect.RouteName, RouteNames.ProviderStart);
            Assert.AreEqual(true, redirect.RouteValues["isFromManage"]);
        }

        [Test]
        public async Task ThenRedirectToStartIfNoActiveFromDateFoundOnNextGlobalFundingRule()
        {
            //Arrange
            var routeModel = new ReservationsRouteModel
            {
                UkPrn = 1,
                IsFromManage = true
            };
            _mockMediator.Setup(x => x.Send(It.IsAny<GetNextUnreadGlobalFundingRuleQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetNextUnreadGlobalFundingRuleResult {Rule = new GlobalRule{Id = 2}});

            //act 
            var redirect = await _controller.Index(routeModel) as RedirectToRouteResult;

            //assert
            Assert.IsNotNull(redirect);
            Assert.AreEqual(redirect.RouteName, RouteNames.ProviderStart);
            Assert.AreEqual(true, redirect.RouteValues["isFromManage"]);
        }
    }
}
