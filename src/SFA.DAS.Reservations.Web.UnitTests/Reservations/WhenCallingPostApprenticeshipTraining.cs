using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.NUnit3;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using SFA.DAS.Encoding;
using SFA.DAS.Reservations.Application.FundingRules.Queries.GetAccountFundingRules;
using SFA.DAS.Reservations.Application.Reservations.Commands.CacheReservationCourse;
using SFA.DAS.Reservations.Application.Reservations.Commands.CacheReservationStartDate;
using SFA.DAS.Reservations.Application.Reservations.Queries.GetCachedReservation;
using SFA.DAS.Reservations.Application.Reservations.Queries.GetCourses;
using SFA.DAS.Reservations.Domain.Courses;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Domain.Rules;
using SFA.DAS.Reservations.Infrastructure.Configuration;
using SFA.DAS.Reservations.Infrastructure.Exceptions;
using SFA.DAS.Reservations.Web.Controllers;
using SFA.DAS.Reservations.Web.Infrastructure;
using SFA.DAS.Reservations.Web.Models;
using SFA.DAS.Reservations.Web.Services;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Reservations.Web.UnitTests.Reservations
{
    public class WhenCallingPostApprenticeshipTraining
    {
        private Course _course;
        private GetCachedReservationResult _cachedReservationResult;
        private Mock<IMediator> _mediator;
        private ReservationsController _controller;
        private IFixture _fixture;
        private Mock<IExternalUrlHelper> _urlHelper;
        private Mock<ITrainingDateService> _mockTrainingDateService;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());

            _course = new Course("1-4-5", "test", 1);
            _cachedReservationResult = _fixture.Create<GetCachedReservationResult>();

            _mockTrainingDateService = new Mock<ITrainingDateService>();
            _mockTrainingDateService.Setup(x => x.GetTrainingDates(It.IsAny<long>()))
                .ReturnsAsync(GetMockTrainingDates());

            _mediator = new Mock<IMediator>();
            _urlHelper = new Mock<IExternalUrlHelper>();
            _controller = new ReservationsController(
                _mediator.Object,
                _mockTrainingDateService.Object,
                Mock.Of<IOptions<ReservationsWebConfiguration>>(),
                Mock.Of<ILogger<ReservationsController>>(),
                Mock.Of<IEncodingService>(),
                _urlHelper.Object);

            _mediator.Setup(mediator => mediator.Send(
                    It.IsAny<GetCachedReservationQuery>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => _cachedReservationResult);

            _mediator.Setup(mediator => mediator.Send(
                    It.IsAny<CacheReservationCourseCommand>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(Unit.Value);

            _mediator.Setup(mediator => mediator.Send(
                    It.IsAny<CacheReservationStartDateCommand>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(Unit.Value);

            _mediator.Setup(m => m.Send(It.IsAny<GetCoursesQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetCoursesResult
                {
                    Courses = new List<Course>
                    {
                        _course
                    }
                });
        }

        [Test, MoqAutoData]
        public async Task When_Model_Error_Then_It_Calls_Start_Date_Service_To_Get_Start_Dates(
            ReservationsRouteModel routeModel,
            ApprenticeshipTrainingFormModel apprenticeshipTrainingFormModel,
            TrainingDateModel trainingDateModel,
            long accountLegalEntityId,
            [Frozen] Mock<IEncodingService> mockEncodingService,
            [Frozen] Mock<ITrainingDateService> mockStartDateService,
            [Frozen] Mock<IMediator> mockMediator,
            [NoAutoProperties] ReservationsController controller)
        {
            apprenticeshipTrainingFormModel.StartDate = JsonConvert.SerializeObject(trainingDateModel);
            controller.ModelState.AddModelError("StartDate", "StartDate");

            mockEncodingService
                .Setup(service => service.Decode(
                    apprenticeshipTrainingFormModel.AccountLegalEntityPublicHashedId,
                    EncodingType.PublicAccountLegalEntityId))
                .Returns(accountLegalEntityId);

            await controller.PostApprenticeshipTraining(routeModel, apprenticeshipTrainingFormModel);

            mockStartDateService.Verify(provider => provider.GetTrainingDates(accountLegalEntityId), Times.Once);
        }

        [Test, MoqAutoData]
        public async Task When_Model_Error_And_Is_Employer_Then_It_Calls_To_Get_Account_Global_Rules(
            ReservationsRouteModel routeModel,
            ApprenticeshipTrainingFormModel apprenticeshipTrainingFormModel,
            TrainingDateModel trainingDateModel,
            long accountLegalEntityId,
            long accountId,
            [Frozen] Mock<IEncodingService> mockEncodingService,
            [Frozen] Mock<ITrainingDateService> mockStartDateService,
            [Frozen] Mock<IMediator> mockMediator,
            [NoAutoProperties] ReservationsController controller)
        {
            apprenticeshipTrainingFormModel.StartDate = JsonConvert.SerializeObject(trainingDateModel);
            controller.ModelState.AddModelError("StartDate", "StartDate");

            mockEncodingService
                .Setup(service => service.Decode(
                    apprenticeshipTrainingFormModel.AccountLegalEntityPublicHashedId,
                    EncodingType.PublicAccountLegalEntityId))
                .Returns(accountLegalEntityId);

            mockEncodingService
                .Setup(service => service.Decode(
                    routeModel.EmployerAccountId,
                    EncodingType.AccountId))
                .Returns(accountId);

            await controller.PostApprenticeshipTraining(routeModel, apprenticeshipTrainingFormModel);

            mockMediator.Verify(provider => provider.Send(It.Is<GetAccountFundingRulesQuery>(x => x.AccountId == accountId), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test, MoqAutoData]
        public async Task When_Model_Error_If_Provider_Then_It_Calls_To_Get_Account_Global_Rules_With_Cached_Reservation_Account_Id(
            ReservationsRouteModel routeModel,
            ApprenticeshipTrainingFormModel apprenticeshipTrainingFormModel,
            TrainingDateModel trainingDateModel,
            GetCachedReservationResult cachedReservationResult,
            long accountLegalEntityId,
            long accountId,
            [Frozen] Mock<IMediator> mockMediator,
            [Frozen] Mock<IEncodingService> mockEncodingService,
            [NoAutoProperties] ReservationsController controller,
            string hashedAccountId)
        {
            apprenticeshipTrainingFormModel.StartDate = JsonConvert.SerializeObject(trainingDateModel);
            controller.ModelState.AddModelError("StartDate", "StartDate");
            routeModel.EmployerAccountId = string.Empty;

            mockMediator
                .Setup(mediator => mediator.Send(It.IsAny<GetCachedReservationQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(cachedReservationResult);
            mockEncodingService
                .Setup(service => service.Decode(
                    apprenticeshipTrainingFormModel.AccountLegalEntityPublicHashedId,
                    EncodingType.PublicAccountLegalEntityId))
                .Returns(accountLegalEntityId);

            mockEncodingService
                .Setup(service => service.Encode(
                    cachedReservationResult.AccountId,
                    EncodingType.AccountId))
                .Returns(hashedAccountId);

            mockEncodingService
                .Setup(service => service.Decode(
                    hashedAccountId,
                    EncodingType.AccountId))
                .Returns(cachedReservationResult.AccountId);

            await controller.PostApprenticeshipTraining(routeModel, apprenticeshipTrainingFormModel);

            mockMediator.Verify(provider => provider.Send(It.Is<GetAccountFundingRulesQuery>(x => x.AccountId == cachedReservationResult.AccountId), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test, MoqAutoData]
        public async Task When_Model_Error_If_Rule_Ends_After_Available_Dates_Show_None(
            ReservationsRouteModel routeModel,
            ApprenticeshipTrainingFormModel apprenticeshipTrainingFormModel,
            TrainingDateModel trainingDateModel,
            GetCachedReservationResult cachedReservationResult,
            long accountLegalEntityId,
            long accountId,
            [Frozen] Mock<IMediator> mockMediator,
            [Frozen] Mock<IEncodingService> mockEncodingService,
            [Frozen] Mock<ITrainingDateService> mockStartDateService,
            [NoAutoProperties] ReservationsController controller)
        {
            apprenticeshipTrainingFormModel.StartDate = JsonConvert.SerializeObject(trainingDateModel);
            controller.ModelState.AddModelError("StartDate", "StartDate");

            mockMediator
                .Setup(mediator => mediator.Send(It.IsAny<GetCachedReservationQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(cachedReservationResult);
            mockEncodingService
                .Setup(service => service.Decode(
                    apprenticeshipTrainingFormModel.AccountLegalEntityPublicHashedId,
                    EncodingType.PublicAccountLegalEntityId))
                .Returns(accountLegalEntityId);

            mockEncodingService
                .Setup(service => service.Decode(
                    routeModel.EmployerAccountId,
                    EncodingType.AccountId))
                .Returns(accountId);

            mockMediator.Setup(mediator => mediator.Send(It.Is<GetAccountFundingRulesQuery>(x => x.AccountId == accountId), It.IsAny<CancellationToken>())).ReturnsAsync(new GetAccountFundingRulesResult
            {
                ActiveRule = new GlobalRule
                {
                    RuleType = GlobalRuleType.DynamicPause,
                    ActiveFrom = DateTime.Now.AddMonths(-1),
                    ActiveTo = DateTime.Now.AddMonths(3)
                }
            });

            mockStartDateService
                .Setup(d => d.GetTrainingDates(It.Is<long>(x => x == accountLegalEntityId)))
                .ReturnsAsync(GetMockTrainingDates());

            var result = await controller.PostApprenticeshipTraining(routeModel, apprenticeshipTrainingFormModel);

            var viewModel = result.Should().BeOfType<ViewResult>()
                .Which.Model.Should().BeOfType<ApprenticeshipTrainingViewModel>()
                .Subject;

            viewModel.PossibleStartDates.Should().BeEmpty();
        }

        [Test, MoqAutoData]
        public async Task When_Model_Error_If_Rule_Ends_Before_Available_Dates_Show(
            ReservationsRouteModel routeModel,
            ApprenticeshipTrainingFormModel apprenticeshipTrainingFormModel,
            TrainingDateModel trainingDateModel,
            GetCachedReservationResult cachedReservationResult,
            long accountLegalEntityId,
            long accountId,
            [Frozen] Mock<IMediator> mockMediator,
            [Frozen] Mock<IEncodingService> mockEncodingService,
            [Frozen] Mock<ITrainingDateService> mockStartDateService,
            [NoAutoProperties] ReservationsController controller)
        {
            apprenticeshipTrainingFormModel.StartDate = JsonConvert.SerializeObject(trainingDateModel);
            controller.ModelState.AddModelError("StartDate", "StartDate");

            mockMediator
                .Setup(mediator => mediator.Send(It.IsAny<GetCachedReservationQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(cachedReservationResult);
            mockEncodingService
                .Setup(service => service.Decode(
                    apprenticeshipTrainingFormModel.AccountLegalEntityPublicHashedId,
                    EncodingType.PublicAccountLegalEntityId))
                .Returns(accountLegalEntityId);

            mockEncodingService
                .Setup(service => service.Decode(
                    routeModel.EmployerAccountId,
                    EncodingType.AccountId))
                .Returns(accountId);

            mockMediator.Setup(mediator => mediator.Send(It.Is<GetAccountFundingRulesQuery>(x => x.AccountId == accountId), It.IsAny<CancellationToken>())).ReturnsAsync(new GetAccountFundingRulesResult
            {
                ActiveRule = new GlobalRule
                {
                    RuleType = GlobalRuleType.DynamicPause,
                    ActiveFrom = DateTime.Now.AddMonths(-1),
                    ActiveTo = DateTime.Now.AddMonths(1)
                }
            });

            mockStartDateService
                .Setup(d => d.GetTrainingDates(It.Is<long>(x => x == accountLegalEntityId)))
                .ReturnsAsync(GetMockTrainingDates());

            var result = await controller.PostApprenticeshipTraining(routeModel, apprenticeshipTrainingFormModel);

            var viewModel = result.Should().BeOfType<ViewResult>()
                .Which.Model.Should().BeOfType<ApprenticeshipTrainingViewModel>()
                .Subject;

            viewModel.PossibleStartDates.Should().HaveCount(2);
        }

        [Test, AutoData]
        public async Task Then_The_Model_Is_Validated_And_Confirmation_Returned(
            ApprenticeshipTrainingFormModel model,
            TrainingDateModel trainingDateModel,
            ReservationsRouteModel routeModel)
        {
            model.StartDate = JsonConvert.SerializeObject(trainingDateModel);
            _controller.ModelState.AddModelError("StartDate", "StartDate");

            var actual = await _controller.PostApprenticeshipTraining(routeModel, model);

            var actualModel = actual as ViewResult;
            Assert.IsNotNull(actualModel);
            Assert.AreEqual("ApprenticeshipTraining", actualModel.ViewName);
        }

        [Test, MoqAutoData]
        public async Task And_Has_Ukprn_Then_Redirects_To_Provider_Route(
            ReservationsRouteModel routeModel,
            TrainingDateModel trainingDateModel,
            ApprenticeshipTrainingFormModel formModel)
        {
            formModel.StartDate = JsonConvert.SerializeObject(trainingDateModel);
            formModel.SelectedCourseId = null;

            var result = await _controller.PostApprenticeshipTraining(routeModel, formModel) as RedirectToRouteResult;

            result.Should().NotBeNull($"result was not a {typeof(RedirectToRouteResult)}");
            result.RouteName.Should().Be(RouteNames.ProviderReview);
        }

        [Test, MoqAutoData]
        public async Task And_No_Ukprn_Then_Redirects_To_Employer_Route(
            ReservationsRouteModel routeModel,
            TrainingDateModel trainingDateModel,
            ApprenticeshipTrainingFormModel formModel)
        {
            formModel.StartDate = JsonConvert.SerializeObject(trainingDateModel);
            formModel.SelectedCourseId = null;
            routeModel.UkPrn = null;

            var result = await _controller.PostApprenticeshipTraining(routeModel, formModel) as RedirectToRouteResult;

            result.Should().NotBeNull($"result was not a {typeof(RedirectToRouteResult)}");
            result.RouteName.Should().Be(RouteNames.EmployerReview);
        }

        [Test, MoqAutoData]
        public async Task And_Has_Ukprn_Then_Caches_Course_And_StartDate(
            ReservationsRouteModel routeModel,
            TrainingDateModel trainingDateModel,
            ApprenticeshipTrainingFormModel formModel)
        {

            formModel.StartDate = JsonConvert.SerializeObject(trainingDateModel);
            formModel.SelectedCourseId = _course.Id;

            await _controller.PostApprenticeshipTraining(routeModel, formModel);

            _mediator.Verify(mediator => mediator.Send(
                    It.Is<CacheReservationCourseCommand>(c =>
                        c.SelectedCourseId.Equals(_course.Id) &&
                        c.UkPrn.Equals(routeModel.UkPrn)),
                    It.IsAny<CancellationToken>()));

            _mediator.Verify(mediator => mediator.Send(
                    It.Is<CacheReservationStartDateCommand>(c =>
                        c.TrainingDate.Equals(trainingDateModel) &&
                        c.UkPrn.Equals(routeModel.UkPrn)),
                    It.IsAny<CancellationToken>()));
        }

        [Test, MoqAutoData]
        public async Task And_No_Ukprn_Then_Caches_StartDate_Only(
            ReservationsRouteModel routeModel,
            TrainingDateModel trainingDateModel,
            ApprenticeshipTrainingFormModel formModel)
        {
            routeModel.UkPrn = null;
            formModel.StartDate = JsonConvert.SerializeObject(trainingDateModel);
            formModel.SelectedCourseId = _course.Id;

            await _controller.PostApprenticeshipTraining(routeModel, formModel);

            _mediator.Verify(mediator => mediator.Send(
                It.Is<CacheReservationCourseCommand>(c =>
                    c.SelectedCourseId.Equals(_course.Id)),
                It.IsAny<CancellationToken>()), Times.Never);

            _mediator.Verify(mediator => mediator.Send(
                It.Is<CacheReservationStartDateCommand>(c =>
                    c.TrainingDate.Equals(trainingDateModel)),
                It.IsAny<CancellationToken>()));
        }

        [Test, MoqAutoData]
        public async Task And_No_Course_Then_Caches_Draft_Reservation(
            ReservationsRouteModel routeModel,
            TrainingDateModel trainingDateModel,
            ApprenticeshipTrainingFormModel formModel)
        {
            formModel.StartDate = JsonConvert.SerializeObject(trainingDateModel);
            formModel.SelectedCourseId = null;

            await _controller.PostApprenticeshipTraining(routeModel, formModel);

            _mediator.Verify(mediator => mediator.Send(
                It.Is<CacheReservationCourseCommand>(c =>
                    c.SelectedCourseId == null),
                It.IsAny<CancellationToken>()));

            _mediator.Verify(mediator => mediator.Send(
                It.Is<CacheReservationStartDateCommand>(c =>
                    c.TrainingDate.Equals(trainingDateModel) &&
                    c.UkPrn.Equals(routeModel.UkPrn)),
                It.IsAny<CancellationToken>()));
        }

        [Test, MoqAutoData]
        public async Task Then_Adds_Guid_To_RouteModel(
            ReservationsRouteModel routeModel,
            TrainingDateModel trainingDateModel,
            ApprenticeshipTrainingFormModel formModel)
        {
            formModel.StartDate = JsonConvert.SerializeObject(trainingDateModel);
            formModel.SelectedCourseId = _course.Id;

            var result = await _controller.PostApprenticeshipTraining(routeModel, formModel) as RedirectToRouteResult;

            result.RouteValues.Should().ContainKey("Id")
                .WhichValue.Should().Be(routeModel.Id);
        }

        [Test, AutoData]//note cannot use moqautodata to construct controller here due to modelmetadata usage.
        public async Task And_Start_Date_Validation_Error_Then_Returns_Validation_Error_Details(
            ReservationsRouteModel routeModel,
            TrainingDateModel trainingDateModel,
            ApprenticeshipTrainingFormModel formModel)
        {
            formModel.StartDate = JsonConvert.SerializeObject(trainingDateModel);
            formModel.SelectedCourseId = _course.Id;

            _mediator
                .Setup(mediator => mediator.Send(It.IsAny<CacheReservationStartDateCommand>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ValidationException(new ValidationResult("Failed", new List<string> { "TrainingStartDate|The TrainingStartDate field is not valid." }), null, null));

            var result = await _controller.PostApprenticeshipTraining(routeModel, formModel);

            Assert.IsNotNull(result);
            var actualViewResult = result as ViewResult;
            Assert.IsNotNull(actualViewResult);
            Assert.IsFalse(actualViewResult.ViewData.ModelState.IsValid);
            Assert.IsTrue(actualViewResult.ViewData.ModelState.ContainsKey("TrainingStartDate"));
        }

        [Test, AutoData] //note cannot use moqautodata to construct controller here due to modelmetadata usage.
        public async Task And_Course_Validation_Error_Then_Returns_Validation_Error_Details(
            ReservationsRouteModel routeModel,
            TrainingDateModel trainingDateModel,
            ApprenticeshipTrainingFormModel formModel)
        {
            formModel.StartDate = JsonConvert.SerializeObject(trainingDateModel);
            formModel.SelectedCourseId = _course.Id;

            _mediator.Setup(mediator =>
                    mediator.Send(It.IsAny<CacheReservationCourseCommand>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ValidationException(
                    new ValidationResult("Failed",
                        new List<string> { "CourseId|The CourseId field is invalid." }), null, null));

            var result = await _controller.PostApprenticeshipTraining(routeModel, formModel);

            Assert.IsNotNull(result);
            var actualViewResult = result as ViewResult;
            Assert.IsNotNull(actualViewResult);
            Assert.IsFalse(actualViewResult.ViewData.ModelState.IsValid);
            Assert.IsTrue(actualViewResult.ViewData.ModelState.ContainsKey("CourseId"));
        }

        [Test, AutoData]
        public async Task Then_If_There_Is_An_Error_The_Model_Is_Correctly_Built_And_Returned_To_The_View(
            string cohortDetailsUrl,
            ReservationsRouteModel routeModel,
            TrainingDateModel trainingDateModel,
            ApprenticeshipTrainingFormModel formModel)
        {
            formModel.FromReview = false;
            _urlHelper.Setup(helper => helper.GenerateCohortDetailsUrl(routeModel.UkPrn,
                    string.Empty, formModel.CohortRef, false, It.IsAny<string>(), string.Empty))
                .Returns(cohortDetailsUrl);
            formModel.StartDate = JsonConvert.SerializeObject(trainingDateModel);
            _controller.ModelState.AddModelError("StartDate", "StartDate");

            var result = await _controller.PostApprenticeshipTraining(routeModel, formModel);

            var actualViewResult = result as ViewResult;
            Assert.IsNotNull(actualViewResult);
            Assert.IsFalse(actualViewResult.ViewData.ModelState.IsValid);
            var actualModel = actualViewResult.Model as ApprenticeshipTrainingViewModel;
            Assert.IsNotNull(actualModel);
            Assert.AreEqual(cohortDetailsUrl, actualModel.BackLink);
        }

        [Test, AutoData]
        public async Task Then_If_There_Is_An_Error_The_Model_Is_Correctly_Built_And_Returned_To_The_View_And_The_Back_Link_Is_Correct(
            string cohortDetailsUrl,
            ReservationsRouteModel routeModel,
            TrainingDateModel trainingDateModel,
            ApprenticeshipTrainingFormModel formModel)
        {
            formModel.FromReview = true;
            formModel.CohortRef = string.Empty;
            formModel.StartDate = JsonConvert.SerializeObject(trainingDateModel);
            _controller.ModelState.AddModelError("StartDate", "StartDate");

            var result = await _controller.PostApprenticeshipTraining(routeModel, formModel);

            var actualViewResult = result as ViewResult;
            Assert.IsNotNull(actualViewResult);
            Assert.IsFalse(actualViewResult.ViewData.ModelState.IsValid);
            var actualModel = actualViewResult.Model as ApprenticeshipTrainingViewModel;
            Assert.IsNotNull(actualModel);
            Assert.AreEqual(RouteNames.ProviderReview, actualModel.BackLink);
        }


        [Test, AutoData]
        public async Task And_CachedReservationNotFoundException_And_Has_Ukprn_Then_Redirect_To_ProviderIndex(
            ReservationsRouteModel routeModel,
            TrainingDateModel trainingDateModel,
            ApprenticeshipTrainingFormModel formModel,
            CachedReservationNotFoundException notFoundException)
        {
            formModel.StartDate = JsonConvert.SerializeObject(trainingDateModel);
            formModel.SelectedCourseId = _course.Id;

            _mediator
                .Setup(mediator => mediator.Send(It.IsAny<GetCachedReservationQuery>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(notFoundException);

            var actual = await _controller.PostApprenticeshipTraining(routeModel, formModel) as RedirectToRouteResult;

            actual.Should().NotBeNull();
            actual.RouteName.Should().Be(RouteNames.ProviderIndex);
        }

        [Test, AutoData]
        public async Task And_CachedReservationNotFoundException_And_No_Ukprn_Then_Redirect_To_EmployerIndex(
            ReservationsRouteModel routeModel,
            TrainingDateModel trainingDateModel,
            ApprenticeshipTrainingFormModel formModel,
            CachedReservationNotFoundException notFoundException)
        {
            formModel.StartDate = JsonConvert.SerializeObject(trainingDateModel);
            formModel.SelectedCourseId = _course.Id;
            routeModel.UkPrn = null;

            _mediator
                .Setup(mediator => mediator.Send(It.IsAny<GetCachedReservationQuery>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(notFoundException);

            var actual = await _controller.PostApprenticeshipTraining(routeModel, formModel) as RedirectToRouteResult;

            actual.Should().NotBeNull();
            actual.RouteName.Should().Be(RouteNames.EmployerIndex);
        }

        private static IEnumerable<TrainingDateModel> GetMockTrainingDates()
        {
            var previousMonth = DateTime.Now.AddMonths(-1);
            var nextMonth = previousMonth.AddMonths(2);
            var nextMonthLastDay = DateTime.DaysInMonth(nextMonth.Year, nextMonth.Month);

            var previousMonthModel = new TrainingDateModel
            {
                StartDate = new DateTime(previousMonth.Year, previousMonth.Month, 1),
                EndDate = new DateTime(nextMonth.Year, nextMonth.Month, nextMonthLastDay)
            };

            var trainingDates = new List<TrainingDateModel>
            {
                previousMonthModel
            };

            for (int i = 0; i < 3; i++)
            {
                var startDate = DateTime.Now.AddMonths(i);
                var endDate = startDate.AddMonths(2);
                trainingDates.Add(new TrainingDateModel
                {
                    StartDate = startDate,
                    EndDate = endDate
                });
            }

            return trainingDates;
        }
    }
}
