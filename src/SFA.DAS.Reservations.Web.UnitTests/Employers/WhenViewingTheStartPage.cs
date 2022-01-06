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
using AutoFixture.AutoMoq;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Options;
using SFA.DAS.Encoding;
using SFA.DAS.Reservations.Application.FundingRules.Queries.GetAccountFundingRules;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Domain.Rules;
using SFA.DAS.Reservations.Domain.Rules.Api;
using SFA.DAS.Reservations.Infrastructure.Configuration;
using SFA.DAS.Reservations.Web.Infrastructure;
using SFA.DAS.Reservations.Web.Models;

namespace SFA.DAS.Reservations.Web.UnitTests.Employers
{
    public class WhenViewingTheStartPage
    {
        private const long ExpectedAccountId = 10;

        private EmployerReservationsController _controller;
        private Mock<IMediator> _mockMediator;
        private Mock<IEncodingService> _mockEncodingService;
        private Mock<IOptions<ReservationsWebConfiguration>> _mockOptions;
        private Mock<IExternalUrlHelper> _externalUrlHelper;
        private Mock<IUrlHelper> _urlHelper;
        private ReservationsRouteModel _routeModel;
        private GlobalRule _testRule;
        
        [SetUp]
        public void Arrange()
        {
            var fixture = new Fixture().Customize(new AutoMoqCustomization{ConfigureMembers = true});
            var config = fixture.Create<ReservationsWebConfiguration>();
            _mockOptions = new Mock<IOptions<ReservationsWebConfiguration>>();
            _mockOptions.SetupGet(options => options.Value)
                .Returns(config);
            _mockMediator = fixture.Freeze<Mock<IMediator>>();
            _mockEncodingService = fixture.Freeze<Mock<IEncodingService>>();
            _externalUrlHelper = fixture.Freeze<Mock<IExternalUrlHelper>>();
            _urlHelper = fixture.Freeze<Mock<IUrlHelper>>();

            _routeModel = fixture.Create<ReservationsRouteModel>();

            _mockEncodingService.Setup(s => s.Decode(_routeModel.EmployerAccountId, EncodingType.AccountId))
                .Returns(ExpectedAccountId);

            _controller = fixture.Create<EmployerReservationsController>();

            _controller.Url = _urlHelper.Object;

            _testRule = new GlobalRule
            {
                Id = 2,
                ActiveFrom = DateTime.Now.AddDays(-2),
                RuleType = GlobalRuleType.FundingPaused
            };
        }

        [Test]
        public async Task ThenRedirectToIndexIfNoFundingRulesExist()
        {
            //arrange
            _mockMediator.Setup(x => x.Send(It.IsAny<GetAccountFundingRulesQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetAccountFundingRulesResult
                {
                    AccountFundingRules = new GetAccountFundingRulesApiResponse
                    {
                        GlobalRules = new List<GlobalRule>()
                    },
                    ActiveRule = null
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
            _mockMediator.Setup(x => x.Send(It.Is<GetAccountFundingRulesQuery>(q => q.AccountId.Equals(ExpectedAccountId)), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetAccountFundingRulesResult
            {
                AccountFundingRules = new GetAccountFundingRulesApiResponse
                {
                    GlobalRules = new List<GlobalRule>
                    {
                        _testRule
                    }
                },
                ActiveRule = _testRule
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
            _testRule.RuleType = GlobalRuleType.ReservationLimit;

            _mockMediator.Setup(x => x.Send(It.Is<GetAccountFundingRulesQuery>(q => q.AccountId.Equals(ExpectedAccountId)), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetAccountFundingRulesResult
                {
                    AccountFundingRules = new GetAccountFundingRulesApiResponse
                    {
                        GlobalRules = new List<GlobalRule>
                        {
                            _testRule
                        }
                    },
                    ActiveRule = _testRule
                });

            //act
            var result = await _controller.Start(_routeModel) as ViewResult;

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("ReservationLimitReached",result.ViewName);
            
        }

        [Test]
        public async Task IfReservationLimitRuleExists_AndNoCohortIsAvailable_ThenUseEmployerManagePageAsBackLink()
        {
            //arrange
            _testRule.RuleType = GlobalRuleType.ReservationLimit;
            _mockMediator.Setup(x => x.Send(It.Is<GetAccountFundingRulesQuery>(q => q.AccountId.Equals(ExpectedAccountId)), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetAccountFundingRulesResult
                {
                    AccountFundingRules = new GetAccountFundingRulesApiResponse
                    {
                        GlobalRules = new List<GlobalRule>
                        {
                            _testRule
                        }
                    },
                    ActiveRule = _testRule
                });

            //No cohort means request came from manage reservations page
            _routeModel.CohortReference = null;

            var expectedBackUrl = "http://www.test.com";

            _urlHelper.Setup(h => h.RouteUrl(It.Is<UrlRouteContext>(c =>
                    c.RouteName.Equals(RouteNames.EmployerManage) &&
                    c.Values.Equals(_routeModel))))
                .Returns(expectedBackUrl);


            //act
            var result = await _controller.Start(_routeModel) as ViewResult;

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedBackUrl, result.Model);
        }

        [Test]
        public async Task IfReservationLimitRuleExists_AndCohortIsAvailable_ThenUseCohortDetailsPageAsBackLink()
        {
            //arrange
            _testRule.RuleType = GlobalRuleType.ReservationLimit;

            _mockMediator.Setup(x => x.Send(It.Is<GetAccountFundingRulesQuery>(q => q.AccountId.Equals(ExpectedAccountId)), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetAccountFundingRulesResult
                {
                    AccountFundingRules = new GetAccountFundingRulesApiResponse
                    {
                        GlobalRules = new List<GlobalRule>
                        {
                            _testRule
                        }
                    },
                    ActiveRule = _testRule
                });

            
            _routeModel.UkPrn = null;

            var expectedBackUrl = "http://www.test.com";

            _externalUrlHelper.Setup(h => h.GenerateCohortDetailsUrl(
                    null,
                    _routeModel.EmployerAccountId,
                    _routeModel.CohortReference, false,
                    It.IsAny<string>()))
                .Returns(expectedBackUrl);

            //act
            var result = await _controller.Start(_routeModel) as ViewResult;

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedBackUrl, result.Model);
        }
    }
}
