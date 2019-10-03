using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Domain.Reservations.Api;
using SFA.DAS.Reservations.Infrastructure.Api;
using SFA.DAS.Reservations.Web.AcceptanceTests.Infrastructure;
using SFA.DAS.Reservations.Web.Controllers;
using SFA.DAS.Reservations.Web.Infrastructure;
using SFA.DAS.Reservations.Web.Models;
using TechTalk.SpecFlow;

namespace SFA.DAS.Reservations.Web.AcceptanceTests.Steps
{
    [Binding]
    public class ReservationSteps : StepsBase
    {
        private string _reviewRedirectUrl;

        public ReservationSteps(TestServiceProvider serviceProvider, TestData testData) : base(serviceProvider, testData)
        {
        }

        [Given(@"I am a non levy employer")]
        public void GivenIAmANonLevyEmployer()
        {
            var controller = Services.GetService<EmployerReservationsController>();
            var confirmLegalEntityViewModel = new ConfirmLegalEntityViewModel
            {
                LegalEntity = TestData.AccountLegalEntity.AccountLegalEntityPublicHashedId
            };

            var result = controller.PostSelectLegalEntity(TestData.ReservationRouteModel, confirmLegalEntityViewModel)
                        .Result as RedirectToRouteResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(RouteNames.EmployerSelectCourse, result.RouteName);
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

            Assert.IsNotNull(result);
            Assert.AreEqual(RouteNames.EmployerApprenticeshipTraining, result.RouteName);
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

            Assert.IsNotNull(result);
            Assert.AreEqual(RouteNames.EmployerReview, result.RouteName);
        }


        [When(@"I review my reservation and confirm")]
        public void WhenIReviewMyReservationAndConfirm()
        {
            var result = PostReviewStep(true) as RedirectToRouteResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(RouteNames.EmployerCompleted, result.RouteName);
        }

        [When(@"I review my reservation and I do not confirm")]
        public void WhenIReviewMyReservationAndIDoNotConfirm()
        {
            var result = PostReviewStep(false) as RedirectResult;

            Assert.IsNotNull(result);
            _reviewRedirectUrl = result.Url;
            
        }


        [Then(@"The reservation is created")]
        public void ThenThenTheReservationIsCreated()
        {
            var apiClient = Services.GetService<IApiClient>();
            var mock = Mock.Get(apiClient);
            
            mock.Verify(x => x.Create<CreateReservationResponse>(It.Is<ReservationApiRequest>(
                c => c.Id.Equals(TestData.ReservationRouteModel.Id) &&
                     c.CourseId.Equals(TestData.Course.Id) &&
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
            Assert.AreEqual($"https://accounts.{TestDataValues.EmployerDashboardUrl}/accounts/{TestDataValues.EmployerAccountId}/teams",_reviewRedirectUrl);
        }

        private IActionResult PostReviewStep(bool reserve)
        {
            var controller = Services.GetService<ReservationsController>();

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