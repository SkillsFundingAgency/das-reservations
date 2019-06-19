using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using SFA.DAS.Encoding;
using SFA.DAS.Reservations.Application.FundingRules.Commands.MarkRuleAsRead;
using SFA.DAS.Reservations.Application.FundingRules.Queries.GetFundingRules;
using SFA.DAS.Reservations.Domain.Rules;
using SFA.DAS.Reservations.Domain.Rules.Api;
using SFA.DAS.Reservations.Infrastructure.Configuration;
using SFA.DAS.Reservations.Web.Controllers;
using SFA.DAS.Reservations.Web.Infrastructure;
using SFA.DAS.Reservations.Web.Models;
using SFA.DAS.Testing.AutoFixture;

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

        [Test, MoqAutoData]
        public async Task ThenSendsCorrectCommand(
            [Frozen] Mock<IMediator> mockMediator,
            EmployerReservationsController controller)
        {
            //arrange
            var expectedRuleId = 12L;
            var expectedTypeOfRule = RuleType.GlobalRule;
            var expectedUserId = "123";

            var claim = new Claim(EmployerClaims.IdamsUserIdClaimTypeIdentifier, expectedUserId);
            controller.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[] {claim}));
            
            //act
            await controller.SaveRuleNotificationChoice(expectedRuleId, expectedTypeOfRule, true);

            //assert
            mockMediator.Verify(m => m.Send(It.Is<MarkRuleAsReadCommand>(c => 
                c.Id.Equals(expectedUserId) &&
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
