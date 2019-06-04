using System;
using System.Collections.Generic;
using System.Threading;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Web.Controllers;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using SFA.DAS.Encoding;
using SFA.DAS.Reservations.Application.FundingRules.Queries.GetFundingRules;
using SFA.DAS.Reservations.Domain.Rules;
using SFA.DAS.Reservations.Infrastructure.Configuration;

namespace SFA.DAS.Reservations.Web.UnitTests.Employers
{
    public class WhenViewingTheStartPage
    {
        private EmployerReservationsController _controller;
        private Mock<IMediator> _mockMediator;
        private Mock<IEncodingService> _mockEncodingService;
        private Mock<IFundingRulesService> _fundingRulesService;

        [SetUp]
        public void Arrange()
        {
            _mockMediator = new Mock<IMediator>();
            _mockEncodingService = new Mock<IEncodingService>();
            _controller = new EmployerReservationsController(_mockMediator.Object, _mockEncodingService.Object, Mock.Of<IOptions<ReservationsWebConfiguration>>());
        }

        [Test]
        public async Task ThenRedirectToIndexIfNoFundingRulesExist()
        {
            //arrange
            _mockMediator.Setup(x => x.Send(It.IsAny<GetFundingRulesQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetFundingRulesResult
                    {
                        AccountRules = new List<ReservationRule>(),
                        GlobalRules = new List<GlobalRule>()
                    });

            //act 
            var view = await _controller.Start() as ViewResult;

            //assert
            Assert.IsNotNull(view);
            Assert.AreEqual(view.ViewName, "Index");
        }

        [Test,MoqAutoData]
        public async Task ThenRedirectToFundingPausedIfFundingRulesExist(
            [Frozen] long accountId)
        {
            //arrange
            _mockMediator.Setup(x => x.Send(It.IsAny<GetFundingRulesQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetFundingRulesResult
                {
                    AccountRules = new List<ReservationRule>(),
                    GlobalRules = new List<GlobalRule>
                    {
                        new GlobalRule {ActiveFrom = DateTime.Now.AddDays(-2)}
                    }
                });

            //act 
            var view = await _controller.Start() as ViewResult;

            //assert
            Assert.IsNotNull(view);
            Assert.AreEqual(view.ViewName, "EmployerFundingPaused");
        }

        [Test,MoqAutoData]
        public async Task IfReservationLimitRuleExists_ThenRedirectToReservationLimitReachedPage(
            [Frozen] Mock<IMediator> mediatorMock,
            [Frozen] long accountId)
        {
            //arrange
            mediatorMock.Setup(x => x.Send(It.IsAny<GetFundingRulesQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetFundingRulesResult()
                {
                    ActiveRule = GlobalRuleType.ReservationLimit
                });

            var controller = new EmployerReservationsController(mediatorMock.Object, _mockEncodingService.Object);

            //act
            var result = await controller.Index(accountId.ToString()) as ViewResult;

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("ReservationLimitReached",result.ViewName);
        }
    }
}
