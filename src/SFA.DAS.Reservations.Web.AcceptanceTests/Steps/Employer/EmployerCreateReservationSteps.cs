using System;
using System.Collections.Generic;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Domain.Reservations.Api;
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
    public class EmployerCreateReservationSteps : StepsBase
    {
        private string _reviewRedirectUrl;

        public EmployerCreateReservationSteps(
            EmployerTestServiceProvider serviceProvider, 
            TestData testData) 
            : base(serviceProvider, testData)
        {
        }

        [Given(@"I am a non levy employer")]
        public void GivenIAmANonLevyEmployer()
        {
            SetupNonLevyEmployerTestData();
        }
        
        [Given(@"I have reached my reservation limit")]
        public void GivenIHaveReachedMyReservationLimit()
        {
            var apiClient = Services.GetService<IApiClient>();
            var mock = Mock.Get(apiClient);
            mock.Setup(x => x.Get<GetAccountFundingRulesApiResponse>(It.IsAny<GetAccountFundingRulesApiRequest>()))
                .ReturnsAsync(new GetAccountFundingRulesApiResponse{GlobalRules = new List<GlobalRule>
                {
                    new GlobalRule
                    {
                        RuleType = GlobalRuleType.ReservationLimit,
                        ActiveFrom = DateTime.UtcNow.AddMonths(-1),
                        Restriction = AccountRestriction.All
                    }
                }});
        }
        
        [When(@"I start the reservation journey")]
        public void WhenIStartTheReservationJourney()
        {
            var controller = Services.GetService<EmployerReservationsController>();
            var urlHelper = Services.GetService<IUrlHelper>();
            var mock = Mock.Get(urlHelper);
            controller.Url = mock.Object;
            TestData.ActionResult = controller.Start(TestData.ReservationRouteModel).Result;
        }
        
        [Given(@"I have chosen a legal entity")]
        public void GivenIHaveChosenALegalEntity()
        {
            var controller = Services.GetService<EmployerReservationsController>();
            var urlHelper = Services.GetService<IUrlHelper>();
            var mock = Mock.Get(urlHelper);
            controller.Url = mock.Object;
            var confirmLegalEntityViewModel = new ConfirmLegalEntityViewModel
            {
                LegalEntity = TestData.AccountLegalEntity.AccountLegalEntityPublicHashedId
            };

            TestData.ActionResult = controller.PostSelectLegalEntity(TestData.ReservationRouteModel, confirmLegalEntityViewModel)
                .Result;

            if (typeof(RedirectToRouteResult) == TestData.ActionResult.GetType())
            {
                var result = TestData.ActionResult as RedirectToRouteResult;

                result.Should().NotBeNull();
                result.RouteName.Should().Be(RouteNames.EmployerSelectCourse);
            }
        }

        [Given(@"I have chosen a course")]
        public void GivenIHaveChosenACourse()
        {
            var controller = Services.GetService<EmployerReservationsController>();
            var postSelectCourseViewModel = new PostSelectCourseViewModel
            {
                SelectedCourseId = TestData.Course.Id,
                ApprenticeTrainingKnown = true
            };

            var result = controller.PostSelectCourse(TestData.ReservationRouteModel, postSelectCourseViewModel)
                    .Result as RedirectToRouteResult;

            result.Should().NotBeNull();
            result.RouteName.Should().Be(RouteNames.EmployerApprenticeshipTraining);
        }

        [Given(@"I have a reservation start date of (.*)")]
        public void GivenIHaveAReservationStartDateOfAugust(string month)
        {
            TestData.BuildTrainingDateModel(month);

            var controller = Services.GetService<ReservationsController>();
            var trainingDateViewModel = new TrainingDateViewModel(TestData.TrainingDate);

            var apprenticeshipTrainingFormModel = new ApprenticeshipTrainingFormModel
            {
                StartDate = trainingDateViewModel.SerializedModel,
                AccountLegalEntityPublicHashedId = TestData.AccountLegalEntity.AccountLegalEntityPublicHashedId
            };

            var result = controller.PostApprenticeshipTraining(TestData.ReservationRouteModel, apprenticeshipTrainingFormModel)
                        .Result as RedirectToRouteResult;

            result.Should().NotBeNull();
            result.RouteName.Should().Be(RouteNames.EmployerReview);
        }

        [When(@"I do not choose a start date")]
        public void GivenIHaveNotChosenAStartDate()
        {
            var controller = Services.GetService<ReservationsController>();
            var trainingDateViewModel = new TrainingDateViewModel(new TrainingDateModel());
            var apprenticeshipTrainingFormModel = new ApprenticeshipTrainingFormModel
            {
                StartDate = trainingDateViewModel.SerializedModel,
                AccountLegalEntityPublicHashedId = TestData.AccountLegalEntity.AccountLegalEntityPublicHashedId
            };
          
            TestData.ActionResult = controller.PostApprenticeshipTraining(TestData.ReservationRouteModel, apprenticeshipTrainingFormModel)
                .Result as ViewResult;
        }

        [When(@"I do not select any training")]
        public void WhenIDoNotSelectWhetherOrNotIKnowTheTraining()
        {
            var controller = Services.GetService<EmployerReservationsController>();
            TestData.ActionResult = controller.PostSelectCourse(TestData.ReservationRouteModel,
                new PostSelectCourseViewModel {ApprenticeTrainingKnown = null, SelectedCourseId = null}).Result;
        }

        [When(@"I review my reservation and confirm")]
        public void WhenIReviewMyReservationAndConfirm()
        {
            var result = PostReviewStep(true) as RedirectToRouteResult;

            result.Should().NotBeNull();
            result.RouteName.Should().Be(RouteNames.EmployerCompleted);
        }

        [When(@"I review my reservation and I do not confirm")]
        public void WhenIReviewMyReservationAndIDoNotConfirm()
        {
            var result = PostReviewStep(false) as RedirectResult;

            result.Should().NotBeNull();
            _reviewRedirectUrl = result.Url;
        }

        [Then(@"I am shown a validation message on the (.*) page")]
        public void ThenIAmShownAValidationMessage(string viewName)
        {
            var result = TestData.ActionResult as ViewResult;
            (result.ViewData.ModelState.ErrorCount!=0).Should().BeTrue();
            result.ViewName.Should().Be(viewName);
        }

        [Then(@"The reservation is created")]
        public void ThenThenTheReservationIsCreated()
        {
            var apiClient = Services.GetService<IApiClient>();
            var mock = Mock.Get(apiClient);
            
            mock.Verify(x => x.Create<CreateReservationResponse>(It.Is<ReservationApiRequest>(
                c => c.Id.Equals(TestData.ReservationRouteModel.Id) &&
                     c.CourseId.Equals(TestData.Course.Id) &&
                     c.UserId.Equals(TestData.UserId) &&
                     c.StartDate.Equals(new DateTime(TestData.TrainingDate.StartDate.Year,TestData.TrainingDate.StartDate.Month,1).ToString("yyyy-MMM-dd"))
            )), Times.Once);
        }

        [Then(@"The reservation is not created")]
        public void ThenTheReservationIsNotCreated()
        {
            var apiClient = Services.GetService<IApiClient>();
            var mock = Mock.Get(apiClient);

            mock.Verify(x => x.Create<CreateReservationResponse>(It.IsAny<ReservationApiRequest>()), Times.Never);
        }

        [Then(@"redirected to employer dashboard")]
        public void ThenRedirectedToEmployerDashboard()
        {
            _reviewRedirectUrl.Should().Be($"https://accounts.{TestDataValues.EmployerDashboardUrl}/accounts/{TestDataValues.EmployerAccountId}/teams");
        }

        [Then(@"I am shown a message saying I have reached my reservation limit")]
        public void ThenIAmShownAMessageSayingIHaveReachedMyReservationLimit()
        {
            var actualViewResult = TestData.ActionResult as ViewResult;
            actualViewResult.Should().NotBeNull();
            actualViewResult.ViewName.Should().Be("ReservationLimitReached");
        }

        private IActionResult PostReviewStep(bool reserve)
        {

            var controller = Services.GetService<ReservationsController>();
            var claim = new Claim(EmployerClaims.IdamsUserIdClaimTypeIdentifier, TestData.UserId.ToString());
            controller.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { claim }));

            var viewModel = new PostReviewViewModel
            {
                Reserve = reserve,
                TrainingDate = TestData.TrainingDate
            };

            var result = controller.PostReview(TestData.ReservationRouteModel, viewModel)
                .Result;
            return result;
        }
    }
}