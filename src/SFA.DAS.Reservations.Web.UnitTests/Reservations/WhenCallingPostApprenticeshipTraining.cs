﻿using System;
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
using SFA.DAS.Reservations.Application.Reservations.Commands.CacheReservationCourse;
using SFA.DAS.Reservations.Application.Reservations.Commands.CacheReservationStartDate;
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
        private IFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());

            _course = new Course("1-4-5","test",1);
            _cachedReservationResult = _fixture.Create<GetCachedReservationResult>();

            _mediator = new Mock<IMediator>();
            _controller = new ReservationsController(
                _mediator.Object, 
                Mock.Of<IStartDateService>(), 
                Mock.Of<IOptions<ReservationsWebConfiguration>>());

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

        [Test, AutoData]
        public async Task Then_The_Model_Is_Validated_And_Confirmation_Returned(ApprenticeshipTrainingFormModel model, ReservationsRouteModel routeModel)
        {
            _controller.ModelState.AddModelError("StartDate", "StartDate");

            var actual = await _controller.PostApprenticeshipTraining(routeModel, model);

            var actualModel = actual as ViewResult;
            Assert.IsNotNull(actualModel);
            Assert.AreEqual("ApprenticeshipTraining", actualModel.ViewName);
        }

        [Test, MoqAutoData]
        public async Task And_Has_Ukprn_Then_Redirects_To_Provider_Route(
            ReservationsRouteModel routeModel,
            StartDateModel startDateModel,
            ApprenticeshipTrainingFormModel formModel)
        {
            formModel.StartDate = JsonConvert.SerializeObject(startDateModel);
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
            formModel.StartDate = JsonConvert.SerializeObject(startDateModel);
            formModel.SelectedCourseId = null;
            routeModel.UkPrn = null;

            var result = await _controller.PostApprenticeshipTraining(routeModel, formModel) as RedirectToRouteResult;
            
            result.Should().NotBeNull($"result was not a {typeof(RedirectToRouteResult)}");
            result.RouteName.Should().Be(RouteNames.EmployerReview);
        }

        [Test, MoqAutoData]
        public async Task And_Has_Ukprn_Then_Caches_Course_And_StartDate(
            ReservationsRouteModel routeModel,
            StartDateModel startDateModel,
            ApprenticeshipTrainingFormModel formModel)
        {

            formModel.StartDate = JsonConvert.SerializeObject(startDateModel);
            formModel.SelectedCourseId = _course.Id;

            await _controller.PostApprenticeshipTraining(routeModel, formModel);

            _mediator.Verify(mediator => mediator.Send(
                    It.Is<CacheReservationCourseCommand>( c => 
                        c.CourseId.Equals(_course.Id)),
                    It.IsAny<CancellationToken>()));

            _mediator.Verify(mediator => mediator.Send(
                    It.Is<CacheReservationStartDateCommand>(c =>
                        c.StartDate.Equals(startDateModel.StartDate.ToString("yyyy-MM")) &&
                        c.StartDateDescription.Equals(startDateModel.ToString())),
                    It.IsAny<CancellationToken>()));
        }

        [Test, MoqAutoData]
        public async Task And_No_Ukprn_Then_Caches_StartDate_Only(
            ReservationsRouteModel routeModel,
            StartDateModel startDateModel,
            ApprenticeshipTrainingFormModel formModel)
        {
            routeModel.UkPrn = null;
            formModel.StartDate = JsonConvert.SerializeObject(startDateModel);
            formModel.SelectedCourseId = _course.Id;

            await _controller.PostApprenticeshipTraining(routeModel, formModel);

            _mediator.Verify(mediator => mediator.Send(
                It.Is<CacheReservationCourseCommand>( c => 
                    c.CourseId.Equals(_course.Id)),
                It.IsAny<CancellationToken>()), Times.Never);

            _mediator.Verify(mediator => mediator.Send(
                It.Is<CacheReservationStartDateCommand>(c =>
                    c.StartDate.Equals(startDateModel.StartDate.ToString("yyyy-MM")) &&
                    c.StartDateDescription.Equals(startDateModel.ToString())),
                It.IsAny<CancellationToken>()));
        }

        [Test, MoqAutoData]
        public async Task And_No_Course_Then_Caches_Draft_Reservation(
            ReservationsRouteModel routeModel,
            StartDateModel startDateModel,
            ApprenticeshipTrainingFormModel formModel)
        {
            formModel.StartDate = JsonConvert.SerializeObject(startDateModel);
            formModel.SelectedCourseId = null;

            await _controller.PostApprenticeshipTraining(routeModel, formModel);

            _mediator.Verify(mediator => mediator.Send(
                It.Is<CacheReservationCourseCommand>( c => 
                    c.CourseId == null),
                It.IsAny<CancellationToken>()));

            _mediator.Verify(mediator => mediator.Send(
                It.Is<CacheReservationStartDateCommand>(c =>
                    c.StartDate.Equals(startDateModel.StartDate.ToString("yyyy-MM")) &&
                    c.StartDateDescription.Equals(startDateModel.ToString())),
                It.IsAny<CancellationToken>()));
        }

        [Test, MoqAutoData]
        public async Task Then_Adds_Guid_To_RouteModel(
            ReservationsRouteModel routeModel,
            StartDateModel startDateModel,
            ApprenticeshipTrainingFormModel formModel)
        {
            formModel.StartDate = JsonConvert.SerializeObject(startDateModel);
            formModel.SelectedCourseId = _course.Id;

            var result = await _controller.PostApprenticeshipTraining(routeModel, formModel) as RedirectToRouteResult;

            result.RouteValues.Should().ContainKey("Id")
                .WhichValue.Should().Be(routeModel.Id);
        }

        [Test, AutoData]//note cannot use moqautodata to construct controller here due to modelmetadata usage.
        public async Task And_Start_Date_Validation_Error_Then_Returns_Validation_Error_Details(
            ReservationsRouteModel routeModel,
            StartDateModel startDateModel,
            ApprenticeshipTrainingFormModel formModel,
            GetCoursesResult coursesResult)
        {
            formModel.StartDate = JsonConvert.SerializeObject(startDateModel);
            formModel.SelectedCourseId = _course.Id;
            
            _mediator.Setup(mediator => mediator.Send(It.IsAny<CacheReservationStartDateCommand>(), It.IsAny<CancellationToken>()))
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
            StartDateModel startDateModel,
            ApprenticeshipTrainingFormModel formModel,
            GetCoursesResult coursesResult)
        {
            formModel.StartDate = JsonConvert.SerializeObject(startDateModel);
            formModel.SelectedCourseId = _course.Id;

            _mediator.Setup(mediator =>
                    mediator.Send(It.IsAny<CacheReservationCourseCommand>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ValidationException(
                    new ValidationResult("Failed",
                        new List<string> {"CourseId|The CourseId field is invalid."}), null, null));

            var result = await _controller.PostApprenticeshipTraining(routeModel, formModel);

            Assert.IsNotNull(result);
            var actualViewResult = result as ViewResult;
            Assert.IsNotNull(actualViewResult);
            Assert.IsFalse(actualViewResult.ViewData.ModelState.IsValid);
            Assert.IsTrue(actualViewResult.ViewData.ModelState.ContainsKey("CourseId"));
        }
    }
}
