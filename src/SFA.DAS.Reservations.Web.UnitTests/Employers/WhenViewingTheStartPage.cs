using System.Collections.Generic;
using System.Threading;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Web.Controllers;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using SFA.DAS.Encoding;
using SFA.DAS.Reservations.Application.FundingRules.Queries.GetFundingRules;
using SFA.DAS.Reservations.Domain.Rules;
using SFA.DAS.Reservations.Domain.Rules.Api;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Testing.AutoFixture;

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
            _controller = new EmployerReservationsController(_mockMediator.Object, _mockEncodingService.Object);
            _fundingRulesService = new Mock<IFundingRulesService>();

        }

        [Test, MoqAutoData]
        public async Task ThenRedirectToIndexIfNoFundingRulesExist(
            [Frozen] long accountId)
        {
            //arrange
            var result = _mockMediator.Setup(x => x.Send(It.IsAny<GetFundingRulesQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetFundingRulesResult()
                    {
                        FundingRules = new GetFundingRulesApiResponse()
                        {
                            GlobalRules = new List<GlobalRule>(),
                            Rules = new List<ReservationRule>()
                        }
                    });

            //act 
            var view = await _controller.Index(accountId.ToString()) as ViewResult;

            //assert
            Assert.IsNotNull(result);
            Assert.AreEqual(view.ViewName, "Index");
        }

        [Test,MoqAutoData]
        public async Task ThenRedirectToFundingPausedIfFundingRulesExist(
            [Frozen] long accountId)
        {
            //arrange
            var result = _mockMediator.Setup(x => x.Send(It.IsAny<GetFundingRulesQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetFundingRulesResult()
                {
                    ActiveRule = GlobalRuleType.FundingPaused
                });

            //act 
            var view = await _controller.Index(accountId.ToString()) as ViewResult;

            //assert
            Assert.IsNotNull(result);
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
