using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using SFA.DAS.Encoding;
using SFA.DAS.Reservations.Application.FundingRules.Commands.MarkRuleAsRead;
using SFA.DAS.Reservations.Domain.Rules.Api;
using SFA.DAS.Reservations.Infrastructure.Configuration;
using SFA.DAS.Reservations.Web.Controllers;
using SFA.DAS.Reservations.Web.Infrastructure;

namespace SFA.DAS.Reservations.Web.UnitTests.Employers
{
    class WhenSavingRuleNotificationChoice
    {
        private EmployerReservationsController _controller;
        private Mock<IMediator> _mockMediator;
        private Mock<IEncodingService> _mockEncodingService;
        private ReservationsWebConfiguration _employerConfig;

        [SetUp]
        public void Arrange()
        {
            _mockMediator = new Mock<IMediator>();
            _mockEncodingService = new Mock<IEncodingService>();
            _employerConfig = new ReservationsWebConfiguration
            {
                EmployerDashboardUrl = "test.com/test"
            };

            var options = new Mock<IOptions<ReservationsWebConfiguration>>();

            options.Setup(o => o.Value).Returns(_employerConfig);

            _controller = new EmployerReservationsController(_mockMediator.Object, _mockEncodingService.Object, options.Object);
        }

        [Test]
        public async Task ThenSendsCorrectCommand()
        {
            //arrange
            var expectedRuleId = 12L;
            var expectedTypeOfRule = RuleType.GlobalRule;
            var expectedUserId = "123";

            var claim = new Claim(EmployerClaims.AccountsClaimsTypeIdentifier, expectedUserId);

            _controller.ControllerContext.HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[] {claim}))
            };

            //act
            await _controller.SaveRuleNotificationChoice(expectedRuleId, expectedTypeOfRule);

            //assert
            _mockMediator.Verify(m => m.Send(It.Is<MarkRuleAsReadCommand>(c => 
                c.Id.Equals(expectedUserId) &&
                c.RuleId.Equals(expectedRuleId) &&
                c.TypeOfRule.Equals(expectedTypeOfRule)), It.IsAny<CancellationToken>()));
        }
    }
}
