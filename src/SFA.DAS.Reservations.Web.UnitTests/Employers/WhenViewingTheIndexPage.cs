using System;
using System.Threading;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Web.Controllers;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using SFA.DAS.Encoding;
using SFA.DAS.Reservations.Application.FundingRules.Queries.GetNextActiveGlobalFundingRule;
using SFA.DAS.Reservations.Domain.Rules;
using SFA.DAS.Reservations.Infrastructure.Configuration;
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

        [SetUp]
        public void Arrange()
        {
            _expectedRule = new GlobalRule {ActiveFrom = DateTime.Now.AddDays(2)};

           var result = new GetNextActiveGlobalFundingRuleResult {Rule = _expectedRule};

            _mockMediator = new Mock<IMediator>();
            _mockEncodingService = new Mock<IEncodingService>();
            _employerConfig = new ReservationsWebConfiguration()
            {
                EmployerDashboardUrl = "test.com/test"
            };

            var options = new Mock<IOptions<ReservationsWebConfiguration>>();

            options.Setup(o => o.Value).Returns(_employerConfig);

            _controller = new EmployerReservationsController(_mockMediator.Object, _mockEncodingService.Object, options.Object);

            _mockMediator.Setup(x => x.Send(It.IsAny<GetNextActiveGlobalFundingRuleQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(result);
        }

        [Test]
        public async Task ThenRedirectToStartIfNoFundingRulesExist()
        {
            //arrange
            _mockMediator.Setup(x => x.Send(It.IsAny<GetNextActiveGlobalFundingRuleQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((GetNextActiveGlobalFundingRuleResult) null);

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
            Assert.AreEqual(_employerConfig.EmployerDashboardUrl, viewModel.BackLink);
        }

        [Test]
        public async Task ThenRedirectToStartIfNoActiveFromDateFoundOnNextGlobalFundingRule()
        {
            //arrange
            _mockMediator.Setup(x => x.Send(It.IsAny<GetNextActiveGlobalFundingRuleQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetNextActiveGlobalFundingRuleResult {Rule = new GlobalRule()});

            //act 
            var redirect = await _controller.Index() as RedirectToActionResult;

            //assert
            Assert.IsNotNull(redirect);
            Assert.AreEqual(redirect.ActionName, "Start");
        }
    }
}
