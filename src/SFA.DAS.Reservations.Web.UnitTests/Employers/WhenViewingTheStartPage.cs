using System;
using System.Collections.Generic;
using System.Threading;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Web.Controllers;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.Extensions.Options;
using SFA.DAS.Encoding;
using SFA.DAS.Reservations.Application.FundingRules.Queries.GetFundingRules;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Domain.Rules;
using SFA.DAS.Reservations.Infrastructure.Configuration;
using SFA.DAS.Reservations.Web.Models;

namespace SFA.DAS.Reservations.Web.UnitTests.Employers
{
    public class WhenViewingTheStartPage
    {
        private EmployerReservationsController _controller;
        private Mock<IMediator> _mockMediator;
        private Mock<IEncodingService> _mockEncodingService;
        private Mock<IOptions<ReservationsWebConfiguration>> _mockOptions;
        private Mock<IExternalUrlHelper> _externalUrlHelper;
        private ReservationsRouteModel _routeModel;

        [SetUp]
        public void Arrange()
        {
            var fixture = new Fixture();
            var config = fixture.Create<ReservationsWebConfiguration>();
            _mockOptions = new Mock<IOptions<ReservationsWebConfiguration>>();
            _mockOptions.SetupGet(options => options.Value)
                .Returns(config);
            _mockMediator = new Mock<IMediator>();
            _mockEncodingService = new Mock<IEncodingService>();
            _externalUrlHelper = new Mock<IExternalUrlHelper>();
            _routeModel = fixture.Create<ReservationsRouteModel>();

            _controller = new EmployerReservationsController(_mockMediator.Object, _mockEncodingService.Object, _mockOptions.Object, _externalUrlHelper.Object);
        }

        [Test]
        public async Task ThenRedirectToIndexIfNoFundingRulesExist()
        {
            //arrange
            _mockMediator.Setup(x => x.Send(It.IsAny<GetFundingRulesQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetFundingRulesResult
                {
                    GlobalRules = new List<GlobalRule>(),
                    AccountRules = new List<ReservationRule>()
                });

            //act 
            var view = await _controller.Start(_routeModel) as ViewResult;

            //assert
            Assert.IsNotNull(view);
            Assert.AreEqual(view.ViewName, "Index");
        }

        [Test]
        public async Task ThenRedirectToFundingPausedIfFundingRulesExist()
        {
            //arrange
            _mockMediator.Setup(x => x.Send(It.IsAny<GetFundingRulesQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetFundingRulesResult
                {
                    AccountRules = new List<ReservationRule>(),
                    GlobalRules = new List<GlobalRule>
                    {
                        new GlobalRule 
                            {
                                Id = 2, 
                                ActiveFrom = DateTime.Now.AddDays(-2),
                                RuleType = GlobalRuleType.FundingPaused
                            }
                    }
                });


            //act 
            var view = await _controller.Start(_routeModel) as ViewResult;

            //assert
            Assert.IsNotNull(view);
            Assert.AreEqual(view.ViewName, "EmployerFundingPaused");
        }

        [Test]
        public async Task IfReservationLimitRuleExists_ThenRedirectToReservationLimitReachedPage()
        {
            //arrange
            _mockMediator.Setup(x => x.Send(It.IsAny<GetFundingRulesQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetFundingRulesResult
                {
                    AccountRules = new List<ReservationRule>(),
                    GlobalRules = new List<GlobalRule>
                    {
                        new GlobalRule
                        {
                            Id = 2,
                            ActiveFrom = DateTime.Now.AddDays(-2),
                            RuleType = GlobalRuleType.ReservationLimit
                        }
                    }
                });
            var expectedBackUrl = "https://test.com";

            _externalUrlHelper.Setup(h => h.GenerateUrl(It.Is<UrlParameters>(p =>
                p.Id.Equals(_routeModel.EmployerAccountId) &&
                p.SubDomain.Equals("accounts") &&
                p.Controller.Equals("teams") &&
                p.Folder.Equals("accounts")))).Returns(expectedBackUrl);

            //act
            var result = await _controller.Start(_routeModel) as ViewResult;

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("ReservationLimitReached",result.ViewName);
            Assert.AreEqual(expectedBackUrl,result.Model);
        }
    }
}
