using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.FundingRules.Commands.MarkRuleAsRead;
using SFA.DAS.Reservations.Domain.Rules.Api;
using SFA.DAS.Reservations.Infrastructure.Configuration;
using SFA.DAS.Reservations.Web.Controllers;
using SFA.DAS.Reservations.Web.Infrastructure;

namespace SFA.DAS.Reservations.Web.UnitTests.Providers
{
    class WhenSavingRuleNotificationChoice
    {
        private ProviderReservationsController _controller;
        private Mock<IMediator> _mockMediator;
        private ReservationsWebConfiguration _config;

        [SetUp]
        public void Arrange()
        {
            _mockMediator = new Mock<IMediator>();
            _config = new ReservationsWebConfiguration
            {
                EmployerDashboardUrl = "test.com/test"
            };

            var options = new Mock<IOptions<ReservationsWebConfiguration>>();

            options.Setup(o => o.Value).Returns(_config);

            _controller = new ProviderReservationsController(_mockMediator.Object, options.Object);
        }

        [Test]
        public async Task ThenSendsCorrectCommand()
        {
            //arrange
            var expectedRuleId = 12L;
            var expectedTypeOfRule = RuleType.GlobalRule;
            var expectedUkPrn = "1234";

            var claim = new Claim(ProviderClaims.ProviderUkprn, expectedUkPrn);

            _controller.ControllerContext.HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[] {claim}))
            };

            //act
            await _controller.SaveRuleNotificationChoice(expectedRuleId, expectedTypeOfRule, true);

            //assert
            _mockMediator.Verify(m => m.Send(It.Is<MarkRuleAsReadCommand>(c => 
                c.Id.Equals(expectedUkPrn) &&
                c.RuleId.Equals(expectedRuleId) &&
                c.TypeOfRule.Equals(expectedTypeOfRule)), It.IsAny<CancellationToken>()));
        }

        [Test]
        public async Task ThenDoesNotSendsCommandIfNotMarkedAsRead()
        {
            //act
            await _controller.SaveRuleNotificationChoice(12, RuleType.GlobalRule, false);

            //assert
            _mockMediator.Verify(m => m.Send(
                It.IsAny<MarkRuleAsReadCommand>(), 
                It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
