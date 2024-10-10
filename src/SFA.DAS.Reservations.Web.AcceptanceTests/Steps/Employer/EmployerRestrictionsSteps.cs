using System;
using System.Collections.Generic;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Domain.Rules;
using SFA.DAS.Reservations.Domain.Rules.Api;
using SFA.DAS.Reservations.Infrastructure.Api;
using SFA.DAS.Reservations.Infrastructure.Services;
using SFA.DAS.Reservations.Web.AcceptanceTests.Infrastructure;
using SFA.DAS.Reservations.Web.Controllers;
using SFA.DAS.Reservations.Web.Infrastructure;
using SFA.DAS.Reservations.Web.Models;
using TechTalk.SpecFlow;

namespace SFA.DAS.Reservations.Web.AcceptanceTests.Steps.Employer
{
    [Binding]
    public class EmployerRestrictionsSteps : StepsBase
    {
        private List<GlobalRule> _globalRules;
        private GlobalRule _globalRule;
        private readonly Claim _claim;

        [Given(@"there are funding restrictions in place")]
        public void GivenThereAreFundingRestrictionsInPlace()
        {
            var apiClient = Services.GetService<IApiClient>();
            var mockClient = Mock.Get(apiClient);
            _globalRule = new GlobalRule
            {
                Id = 1,
                RuleType = GlobalRuleType.FundingPaused,
                ActiveFrom = DateTime.UtcNow.AddDays(-5),
                Restriction = AccountRestriction.NonLevy
            };
            _globalRules = new List<GlobalRule>{ _globalRule };
            mockClient
                .Setup(x => x.Get<GetAccountFundingRulesApiResponse>(It.IsAny<GetAccountFundingRulesApiRequest>()))
                .ReturnsAsync(new GetAccountFundingRulesApiResponse
                {
                    GlobalRules = _globalRules
                });
        }

        [Given(@"there are upcoming funding restrictions in place")]
        public void GivenThereAreUpcomingFundingRestrictionsInPlace()
        {
            var apiClient = Services.GetService<IApiClient>();
            var mockClient = Mock.Get(apiClient);
            _globalRule = new GlobalRule
            {
                Id = 1,
                RuleType = GlobalRuleType.FundingPaused,
                ActiveFrom = DateTime.UtcNow.AddDays(5),
                Restriction = AccountRestriction.NonLevy
            };
            _globalRules = new List<GlobalRule>{ _globalRule };
            mockClient
                .Setup(x => x.Get<GetFundingRulesApiResponse>(It.IsAny<GetFundingRulesApiRequest>()))
                .ReturnsAsync(new GetFundingRulesApiResponse { GlobalRules = _globalRules });
        }

        [Then(@"I am shown that there are restrictions in place")]
        public void ThenIAmShownThatThereAreRestrictionsInPlace()
        {
            var expectedViewName = "EmployerFundingPaused";
            var actualView = TestData.ActionResult as ViewResult;
            actualView.Should().NotBeNull();
            expectedViewName.Should().Be(actualView?.ViewName);
        }

        [Then(@"I am shown that there are upcoming restrictions in place")]
        public void ThenIAmShownThatThereAreUpcomingRestrictionsInPlace()
        {
            var mockUrlHelper = new Mock<IUrlHelper>();
            mockUrlHelper.Setup(x => x.RouteUrl(It.IsAny<UrlRouteContext>())).Returns("https://testUrl.local");
            
            var expectedStartResultViewName = "Index";
            var actualView = TestData.ActionResult as ViewResult;
            actualView.Should().NotBeNull();
            expectedStartResultViewName.Should().Be(actualView.ViewName);

            var expectedIndexResultViewName = "FundingRestrictionNotification";
            var controller = Services.GetService<EmployerReservationsController>();
            controller.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { _claim }));
            controller.Url = mockUrlHelper.Object;
            TestData.ActionResult = controller.Index(TestData.ReservationRouteModel).Result;
            TestData.ActionResult.Should().NotBeNull();
            expectedIndexResultViewName.Should().Be((TestData.ActionResult as ViewResult)?.ViewName);
        }

        [Then(@"I am able to dismiss them")]
        public void ThenIAmAbleToDismissThem()
        {
            var controller = Services.GetService<ReservationsController>();
            controller.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { _claim }));
            var fundingRestrictionViewModel = new FundingRestrictionNotificationViewModel
            {
                RuleId = _globalRule.Id,
                TypeOfRule = RuleType.GlobalRule,
                MarkRuleAsRead = true,
                IsProvider = false,
                RouteName = RouteNames.EmployerStart
            };
            TestData.ActionResult = controller.SaveRuleNotificationChoice(TestData.ReservationRouteModel, fundingRestrictionViewModel).Result;
            TestData.ActionResult.Should().NotBeNull();
            (TestData.ActionResult as RedirectToRouteResult)?.RouteName.Should().Be(RouteNames.EmployerStart);
        }

        public EmployerRestrictionsSteps(EmployerTestServiceProvider serviceProvider, TestData testData) : base(serviceProvider, testData)
        {
            _claim = new Claim(EmployerClaims.IdamsUserIdClaimTypeIdentifier, TestData.UserId.ToString());
        }
    }
}
