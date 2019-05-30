using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.FundingRules.Queries.GetNextUnreadGlobalFundingRule;
using SFA.DAS.Reservations.Domain.Rules;
using SFA.DAS.Reservations.Infrastructure.Configuration;
using SFA.DAS.Reservations.Web.Controllers;
using SFA.DAS.Reservations.Web.Models;

namespace SFA.DAS.Reservations.Web.UnitTests.Providers
{
    public class WhenViewingTheIndexPage
    {
        private ProviderReservationsController _controller;
        private Mock<IMediator> _mockMediator;
        private GlobalRule _expectedRule;
        private ReservationsWebConfiguration _config;

        [SetUp]
        public void Arrange()
        {
            _expectedRule = new GlobalRule {ActiveFrom = DateTime.Now.AddDays(2)};

           var result = new GetNextUnreadGlobalFundingRuleResult {Rule = _expectedRule};

            _mockMediator = new Mock<IMediator>();
            _config = new ReservationsWebConfiguration
            {
                DashboardUrl = "test.com/test"
            };

            var options = new Mock<IOptions<ReservationsWebConfiguration>>();

            options.Setup(o => o.Value).Returns(_config);

            _controller = new ProviderReservationsController(_mockMediator.Object, options.Object);

            _mockMediator.Setup(x => x.Send(It.IsAny<GetNextUnreadGlobalFundingRuleQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(result);
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
            Assert.AreEqual(_config.DashboardUrl, viewModel.BackLink);
        }

        [Test]
        public async Task ThenRedirectToStartIfNoActiveFromDateFoundOnNextGlobalFundingRule()
        {
            //arrange
            _mockMediator.Setup(x => x.Send(It.IsAny<GetNextUnreadGlobalFundingRuleQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetNextUnreadGlobalFundingRuleResult {Rule = new GlobalRule()});

            //act 
            var redirect = await _controller.Index() as RedirectToActionResult;

            //assert
            Assert.IsNotNull(redirect);
            Assert.AreEqual(redirect.ActionName, "Start");
        }
    }
}
