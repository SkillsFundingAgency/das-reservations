using System;
using System.Security.Claims;
using System.Threading;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Web.Controllers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using SFA.DAS.Encoding;
using SFA.DAS.Reservations.Application.FundingRules.Queries.GetNextUnreadGlobalFundingRule;
using SFA.DAS.Reservations.Domain.Rules;
using SFA.DAS.Reservations.Domain.Rules.Api;
using SFA.DAS.Reservations.Infrastructure.Configuration;
using SFA.DAS.Reservations.Web.Infrastructure;
using SFA.DAS.Reservations.Web.Models;

namespace SFA.DAS.Reservations.Web.UnitTests.Employers
{
    public class WhenViewingTheIndexPage
    {
        private EmployerReservationsController _controller;
        private Mock<IMediator> _mockMediator;
        private Mock<IEncodingService> _mockEncodingService;
        private GlobalRule _expectedRule;
        private ReservationsWebConfiguration _employerConfig;
        private string ExpectedUserId = "123";

        [SetUp]
        public void Arrange()
        {
            _expectedRule = new GlobalRule
            {
                Id = 2, 
                ActiveFrom = DateTime.Now.AddDays(2)
            };

           var result = new GetNextUnreadGlobalFundingRuleResult {Rule = _expectedRule};

            _mockMediator = new Mock<IMediator>();
            _mockEncodingService = new Mock<IEncodingService>();
            _employerConfig = new ReservationsWebConfiguration()
            {
                EmployerDashboardUrl = "test.com/test"
            };

            var options = new Mock<IOptions<ReservationsWebConfiguration>>();

            options.Setup(o => o.Value).Returns(_employerConfig);

            _controller = new EmployerReservationsController(_mockMediator.Object, _mockEncodingService.Object, options.Object);

            _mockMediator.Setup(x => x.Send(It.IsAny<GetNextUnreadGlobalFundingRuleQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(result);

            var claim = new Claim(EmployerClaims.IdamsUserIdClaimTypeIdentifier, ExpectedUserId);

            _controller.ControllerContext.HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[] {claim}))
            };
        }

        [Test]
        public async Task ThenChecksIfRelatedUnreadRulesExists()
        {
            //act 
            await _controller.Index();

            //assert
            _mockMediator.Verify(m => m.Send(It.Is<GetNextUnreadGlobalFundingRuleQuery>(
                q => q.Id.Equals(ExpectedUserId)),It.IsAny<CancellationToken>()));
        }

        [Test]
        public async Task ThenRedirectToStartIfNoFundingRulesExist()
        {
            //arrange
            _mockMediator.Setup(x => x.Send(It.IsAny<GetNextUnreadGlobalFundingRuleQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((GetNextUnreadGlobalFundingRuleResult) null);

            //act 
            var redirect = await _controller.Index() as RedirectToActionResult;

            //assert
            Assert.IsNotNull(redirect);
            Assert.AreEqual(redirect.ActionName, "Start");
        }

        [Test]
        public async Task ThenRedirectToFundingPausedIfFundingRulesExist()
        {
            //act 
            var view = await _controller.Index() as ViewResult;

            var viewModel = view?.Model as FundingRestrictionNotificationViewModel;

            //assert
            Assert.IsNotNull(view);
            Assert.IsNotNull(viewModel);
            Assert.AreEqual(view.ViewName, "FundingRestrictionNotification");
            Assert.AreEqual(_expectedRule.Id, viewModel.RuleId);
            Assert.AreEqual(RuleType.GlobalRule, viewModel.TypeOfRule);
            Assert.AreEqual(_expectedRule.ActiveFrom, viewModel.RestrictionStartDate);
            Assert.AreEqual(_employerConfig.EmployerDashboardUrl, viewModel.BackLink);
        }

        [Test]
        public async Task ThenRedirectToStartIfNoIdFoundOnNextGlobalFundingRule()
        {
            //arrange
            _mockMediator.Setup(x => x.Send(It.IsAny<GetNextUnreadGlobalFundingRuleQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetNextUnreadGlobalFundingRuleResult {Rule = new GlobalRule{ActiveFrom = DateTime.Now}});

            //act 
            var redirect = await _controller.Index() as RedirectToActionResult;

            //assert
            Assert.IsNotNull(redirect);
            Assert.AreEqual(redirect.ActionName, "Start");
        }

        [Test]
        public async Task ThenRedirectToStartIfNoActiveFromDateFoundOnNextGlobalFundingRule()
        {
            //arrange
            _mockMediator.Setup(x => x.Send(It.IsAny<GetNextUnreadGlobalFundingRuleQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetNextUnreadGlobalFundingRuleResult {Rule = new GlobalRule{Id = 2}});

            //act 
            var redirect = await _controller.Index() as RedirectToActionResult;

            //assert
            Assert.IsNotNull(redirect);
            Assert.AreEqual(redirect.ActionName, "Start");
        }
    }
}
