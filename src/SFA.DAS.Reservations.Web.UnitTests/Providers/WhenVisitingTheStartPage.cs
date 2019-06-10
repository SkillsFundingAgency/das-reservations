using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.Employers.Queries;
using SFA.DAS.Reservations.Application.FundingRules.Queries.GetFundingRules;
using SFA.DAS.Reservations.Domain.Employers;
using SFA.DAS.Reservations.Domain.Rules;
using SFA.DAS.Reservations.Infrastructure.Configuration;
using SFA.DAS.Reservations.Web.Controllers;

namespace SFA.DAS.Reservations.Web.UnitTests.Providers
{
    public class WhenVisitingTheStartPage
    {
        private ProviderReservationsController _controller;
        private Mock<IMediator> _mediator;
        private const uint ukPrn = 1234;

        [SetUp]
        public void Arrange()
        {
            _mediator = new Mock<IMediator>();

            _controller = new ProviderReservationsController(_mediator.Object, Mock.Of<IOptions<ReservationsWebConfiguration>>());

            _mediator.Setup(m => m.Send(It.IsAny<GetFundingRulesQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetFundingRulesResult
                {
                    AccountRules = new List<ReservationRule>(),
                    GlobalRules = new List<GlobalRule>()
                });
            _mediator.Setup(m => m.Send(It.IsAny<GetTrustedEmployersQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetTrustedEmployersResponse
                {
                    Employers = new List<Employer> { new Employer() }
                });
        }

        [Test]
        public async Task ThenWillBeRoutedToProviderStartPage()
        {
            //Act
            var result = await _controller.Start(123) as ViewResult;

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.ViewName);
           
        }

        [Test]
        public async Task ThenWillBeRedirectedToFundingStoppedPageIfGlobalRuleExists()
        {
            //Arrange
            _mediator.Setup(m => m.Send(It.IsAny<GetFundingRulesQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetFundingRulesResult
                {
                    AccountRules = new List<ReservationRule>(),
                    GlobalRules = new List<GlobalRule>
                    {
                        new GlobalRule{ActiveFrom = DateTime.Now.AddDays(-2)}
                    }
                });

            //Act
            var result = await _controller.Start(123) as ViewResult;

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("ProviderFundingPaused", result.ViewName);
        }

        [Test]
        public async Task Then_The_No_Permissions_View_Is_Shown_If_You_Have_No_Employers_To_Act_On_Behalf_Of()
        {
            //Arrange
            _mediator.Setup(m => m.Send(It.IsAny<GetTrustedEmployersQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetTrustedEmployersResponse
                {
                    Employers = new List<Employer> ()
                });

            //Act
            var result = await _controller.Start(123) as ViewResult;

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("NoPermissions", result.ViewName);
        }
    }
}
