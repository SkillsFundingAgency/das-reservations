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
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.Reservations.Commands;
using SFA.DAS.Reservations.Application.Reservations.Queries;
using SFA.DAS.Reservations.Application.Reservations.Queries.GetCachedReservation;
using SFA.DAS.Reservations.Application.Reservations.Queries.GetCourses;
using SFA.DAS.Reservations.Domain.Courses;
using SFA.DAS.Reservations.Infrastructure.Configuration.Configuration;
using SFA.DAS.Reservations.Web.Controllers;
using SFA.DAS.Reservations.Web.Infrastructure;
using SFA.DAS.Reservations.Web.Models;
using SFA.DAS.Reservations.Web.Services;

namespace SFA.DAS.Reservations.Web.UnitTests.Reservations
{
    public class WhenCallingPostApprenticeshipTraining
    {
        private Course _course;
        private GetCachedReservationResult _cachedReservationResult;
        private Mock<IMediator> _mediator;
        private ReservationsController _controller;

        [SetUp]
        public void Arrange()
        {
            var fixture = new Fixture().Customize(new AutoMoqCustomization());

            _course = fixture.Create<Course>();
            _cachedReservationResult = fixture.Create<GetCachedReservationResult>();

            _mediator = new Mock<IMediator>();
            _controller = new ReservationsController(_mediator.Object, Mock.Of<IStartDateService>(), Mock.Of<IOptions<ReservationsWebConfiguration>>());

            _mediator.Setup(mediator => mediator.Send(
                    It.IsAny<GetCachedReservationQuery>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => _cachedReservationResult);

            _mediator.Setup(mediator => mediator.Send(
                    It.IsAny<CacheCreateReservationCommand>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => new CacheReservationResult{Id = _cachedReservationResult.Id});

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
        public async Task And_Has_Ukprn_Then_Redirects_To_Provider_Route(
            ReservationsRouteModel routeModel,
            StartDateModel startDateModel,
            ApprenticeshipTrainingFormModel formModel)
        {
            formModel.TrainingStartDate = JsonConvert.SerializeObject(startDateModel);
            formModel.SelectedCourseId = null;

            var result = await _controller.PostApprenticeshipTraining(routeModel, formModel) as RedirectToRouteResult;
            
            result.Should().NotBeNull($"result was not a {typeof(RedirectToRouteResult)}");
            result.RouteName.Should().Be(RouteNames.ProviderReview);
        }

        [Test, MoqAutoData]
        public async Task And_No_Ukprn_Then_Redirects_To_Employer_Route(
            ReservationsRouteModel routeModel,
            StartDateModel startDateModel,
            ApprenticeshipTrainingFormModel formModel)
        {
            formModel.TrainingStartDate = JsonConvert.SerializeObject(startDateModel);
            formModel.SelectedCourseId = null;
            routeModel.UkPrn = null;

            var result = await _controller.PostApprenticeshipTraining(routeModel, formModel) as RedirectToRouteResult;
            
            result.Should().NotBeNull($"result was not a {typeof(RedirectToRouteResult)}");
            result.RouteName.Should().Be(RouteNames.EmployerReview);
        }

        [Test, MoqAutoData]
        public void Then_Throw_Error_If_No_Reservation_Found(
            ReservationsRouteModel routeModel,
            StartDateModel startDateModel,
            ApprenticeshipTrainingFormModel formModel)
        {
            _mediator.Setup(mediator => mediator.Send(
                    It.IsAny<GetCachedReservationQuery>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => null);

            formModel.TrainingStartDate = JsonConvert.SerializeObject(startDateModel);
            formModel.SelectedCourseId = _course.Id;

            Assert.ThrowsAsync<ArgumentException>(() => _controller.PostApprenticeshipTraining(routeModel, formModel));
        }

        [Test, MoqAutoData]
        public async Task Then_Caches_Draft_Reservation(
            ReservationsRouteModel routeModel,
            StartDateModel startDateModel,
            ApprenticeshipTrainingFormModel formModel)
        {
            formModel.TrainingStartDate = JsonConvert.SerializeObject(startDateModel);
            formModel.SelectedCourseId = _course.Id;

            await _controller.PostApprenticeshipTraining(routeModel, formModel);

            _mediator.Verify(mediator => 
                mediator.Send(It.Is<CacheCreateReservationCommand>(command => 
                    command.AccountId == _cachedReservationResult.AccountId &&
                    command.StartDate == startDateModel.StartDate.ToString("yyyy-MM") &&
                    command.StartDateDescription == startDateModel.ToString() &&
                    command.CourseId == _course.Id &&
                    command.AccountLegalEntityPublicHashedId == cacheResult.AccountLegalEntityPublicHashedId &&
                    command.CourseDescription == _course.CourseDescription &&
                    command.AccountLegalEntityPublicHashedId == _cachedReservationResult.AccountLegalEntityPublicHashedId
                    ), It.IsAny<CancellationToken>()));
        }

        [Test, MoqAutoData]
        public async Task Then_Gets_Employer_Information_From_Cache(
            ReservationsRouteModel routeModel,
            StartDateModel startDateModel,
            ApprenticeshipTrainingFormModel formModel)
        {
            var expectedId = Guid.NewGuid();
            routeModel.Id = expectedId;
            
            formModel.TrainingStartDate = JsonConvert.SerializeObject(startDateModel);
            formModel.SelectedCourseId = _course.Id;

            await _controller.PostApprenticeshipTraining(routeModel, formModel);

            _mediator.Verify(mediator => 
                mediator.Send(It.Is<CacheCreateReservationCommand>(command => 
                    command.AccountLegalEntityId.Equals(_cachedReservationResult.AccountLegalEntityId) &&
                    command.AccountLegalEntityName.Equals(_cachedReservationResult.AccountLegalEntityName) &&
                    command.AccountId == _cachedReservationResult.AccountId &&
                    command.StartDate == startDateModel.StartDate.ToString("yyyy-MM") &&
                    command.StartDateDescription == startDateModel.ToString() &&
                    command.CourseId == _course.Id &&
                    command.CourseDescription == _course.CourseDescription
                ), It.IsAny<CancellationToken>()));

            _mediator.Verify(mediator => mediator.Send(
                It.Is<GetCachedReservationQuery>(q => q.Id.Equals(expectedId)),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test, MoqAutoData]
        public async Task And_No_Course_Then_Caches_Draft_Reservation(
            ReservationsRouteModel routeModel,
            StartDateModel startDateModel,
            ApprenticeshipTrainingFormModel formModel)
        {
            formModel.TrainingStartDate = JsonConvert.SerializeObject(startDateModel);
            formModel.SelectedCourseId = null;

            await _controller.PostApprenticeshipTraining(routeModel, formModel);

            _mediator.Verify(mediator => 
                mediator.Send(It.Is<CacheCreateReservationCommand>(command => 
                    command.AccountId == _cachedReservationResult.AccountId &&
                    command.StartDate == startDateModel.StartDate.ToString("yyyy-MM") &&
                    command.StartDateDescription == startDateModel.ToString() &&
                    command.CourseId == null &&
                    command.CourseDescription == "Unknown"
                ), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test, MoqAutoData]
        public async Task Then_Adds_Guid_To_RouteModel(
            ReservationsRouteModel routeModel,
            StartDateModel startDateModel,
            ApprenticeshipTrainingFormModel formModel)
        {
            formModel.TrainingStartDate = JsonConvert.SerializeObject(startDateModel);
            formModel.SelectedCourseId = _course.Id;

            var result = await _controller.PostApprenticeshipTraining(routeModel, formModel) as RedirectToRouteResult;

            result.RouteValues.Should().ContainKey("Id")
                .WhichValue.Should().Be(_cachedReservationResult.Id);
        }

        [Test, AutoData]//note cannot use moqautodata to construct controller here due to modelmetadata usage.
        public async Task And_Validation_Error_Then_Returns_Validation_Error_Details(
            ReservationsRouteModel routeModel,
            StartDateModel startDateModel,
            ApprenticeshipTrainingFormModel formModel,
            GetCoursesResult coursesResult)
        {
            formModel.TrainingStartDate = JsonConvert.SerializeObject(startDateModel);
            formModel.SelectedCourseId = _course.Id;
            
            _mediator.Setup(mediator => mediator.Send(It.IsAny<CacheCreateReservationCommand>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ValidationException(new ValidationResult("Failed", new List<string> { "TrainingStartDate|The TrainingStartDate field is not valid." }), null, null));
           
            var result = await _controller.PostApprenticeshipTraining(routeModel, formModel);
            
            Assert.IsNotNull(result);
            var actualViewResult = result as ViewResult;
            Assert.IsNotNull(actualViewResult);
            Assert.IsFalse(actualViewResult.ViewData.ModelState.IsValid);
            Assert.IsTrue(actualViewResult.ViewData.ModelState.ContainsKey("TrainingStartDate"));
        }

        [Test, MoqAutoData]
        public async Task Then_Gets_Course_Details_From_Service(
            ReservationsRouteModel routeModel,
            StartDateModel startDateModel,
            ApprenticeshipTrainingFormModel formModel)
        {
            //Assign
            formModel.SelectedCourseId = _course.Id;
            formModel.TrainingStartDate = JsonConvert.SerializeObject(startDateModel);

            //Act
            await _controller.PostApprenticeshipTraining(routeModel, formModel);

            //Assert
            _mediator.Verify(mediator => 
                mediator.Send(It.Is<CacheCreateReservationCommand>(command => 
                    command.CourseId == _course.Id &&
                    command.CourseDescription == _course.CourseDescription
                ), It.IsAny<CancellationToken>()), Times.Once);
            
        }

        [Test, MoqAutoData]
        public void Then_If_Course_Details_Not_Found_Will_Throw_Exception(
            ReservationsRouteModel routeModel,
            StartDateModel startDateModel,
            ApprenticeshipTrainingFormModel formModel)
        {
            //Assign
            _mediator.Setup(m => m.Send(It.IsAny<GetCoursesQuery>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(new GetCoursesResult{Courses = new List<Course>()});

            formModel.SelectedCourseId = _course.Id;
            formModel.TrainingStartDate = JsonConvert.SerializeObject(startDateModel);

            //Act + Assert
            Assert.ThrowsAsync<ArgumentException>(() => _controller.PostApprenticeshipTraining(routeModel, formModel));
        }
    }
}
