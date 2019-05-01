using System.Collections;
using System.Collections.Generic;
using System.Threading;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.Reservations.Services;
using SFA.DAS.Reservations.Web.Controllers;
using System.Threading.Tasks;
using Microsoft.Extensions.Primitives;
using NLog;
using NLog.Web.LayoutRenderers;
using SFA.DAS.Reservations.Application.FundingRules.Queries.GetFundingRules;
using SFA.DAS.Reservations.Domain.Rules;
using SFA.DAS.Reservations.Domain.Rules.Api;

namespace SFA.DAS.Reservations.Web.UnitTests.Employers
{
    public class WhenViewingTheStartPage
    {
        private EmployerReservationsController _controller;
        private Mock<IMediator> _mediator;
        private Mock<IHashingService> _IHashingService;

        [SetUp]
        public void Arrange()
        {
            _mediator = new Mock<IMediator>();
            _IHashingService = new Mock<IHashingService>();
            _controller = new EmployerReservationsController(_mediator.Object, _IHashingService.Object);
            
        }

        
        [Test]
        public async Task ThenRedirectToIndexIfNoFundingRulesExist()
        {
            //arrange
            var result = _mediator.Setup(x => x.Send(It.IsAny<GetFundingRulesQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetFundingRulesResult()
                    {
                        FundingRules = new GetFundingRulesApiResponse()
                        {
                            GlobalRules = new List<GlobalRule>(),
                            Rules = new List<ReservationRule>()
                            
                        }
                    });

            //act 
            var view = await _controller.Index() as ViewResult;

            //assert
            Assert.IsNotNull(result);
            Assert.AreEqual(view.ViewName, "Index");

        }

        [Test]
        public async Task ThenRedirectToFundingPausedIfFundingRulesExist()
        {
            //arrange
            var result = _mediator.Setup(x => x.Send(It.IsAny<GetFundingRulesQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetFundingRulesResult()
                {
                    FundingRules = new GetFundingRulesApiResponse()
                    {
                        GlobalRules = new List<GlobalRule>(){new GlobalRule()},
                        Rules = new List<ReservationRule>()

                    }
                });

            //act 
            var view = await _controller.Index() as ViewResult;

            //assert
            Assert.IsNotNull(result);
            Assert.AreEqual(view.ViewName, "EmployerFundingPaused");

        }

    }
}
