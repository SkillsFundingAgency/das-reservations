using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.FundingRules.Queries.GetFundingRules;
using SFA.DAS.Reservations.Domain.Rules;
using SFA.DAS.Reservations.Domain.Rules.Api;
using SFA.DAS.Reservations.Web.Controllers;

namespace SFA.DAS.Reservations.Web.UnitTests.Providers
{
    public class WhenVisitingTheLandingPage
    {
        private ProviderReservationsController _controller;
        private Mock<IMediator> _mediator;

        [SetUp]
        public void Arrange()
        {
            _mediator = new Mock<IMediator>();

            _controller = new ProviderReservationsController(_mediator.Object);
        }
        
        [Test]
        public async Task ThenWillBeRoutedToProviderLandingPage()
        {
            //Arrange
            _mediator.Setup(m => m.Send(It.IsAny<GetFundingRulesQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetFundingRulesResult
                {
                    FundingRules = new GetFundingRulesApiResponse
                    {
                        Rules = new List<ReservationRule>(),
                        GlobalRules = new List<GlobalRule>()
                    }
                });

            //Act
            var result = await _controller.Index() as ViewResult;

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("index", result.ViewName);
        }

        [Test]
        public async Task ThenWillBeRedirectedToFundingStoppedPageIfGlobalRuleExists()
        {
            //Arrange
            _mediator.Setup(m => m.Send(It.IsAny<GetFundingRulesQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetFundingRulesResult
                {
                    FundingRules = new GetFundingRulesApiResponse
                    {
                        Rules = new List<ReservationRule>(),
                        GlobalRules = new List<GlobalRule>
                        {
                            new GlobalRule()
                        }
                    }
                });

            //Act
            var result = await _controller.Index() as ViewResult;

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("ProviderFundingPaused", result.ViewName);
        }
    }
}
