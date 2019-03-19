using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.Reservations.Commands;
using SFA.DAS.Reservations.Application.Reservations.Queries.GetCourses;
using SFA.DAS.Reservations.Domain.Courses;
using SFA.DAS.Reservations.Web.Controllers;
using SFA.DAS.Reservations.Web.Models;
using SFA.DAS.Reservations.Web.Services;

namespace SFA.DAS.Reservations.Web.UnitTests.Reservations
{
    [TestFixture]
    public class WhenCallingPostApprenticeshipTraining
    {
        [Test, MoqAutoData]
        public async Task And_Has_Ukprn_Then_Redirects_To_Provider_Route(
            ReservationsRouteModel routeModel,
            StartDateModel startDateModel,
            Course course,
            ApprenticeshipTrainingFormModel formModel,
            ReservationsController controller)
        {
            formModel.TrainingStartDate = JsonConvert.SerializeObject(startDateModel);
            formModel.CourseId = JsonConvert.SerializeObject(course);

            var result = await controller.PostApprenticeshipTraining(routeModel, formModel) as RedirectToRouteResult;
            
            result.Should().NotBeNull($"result was not a {typeof(RedirectToRouteResult)}");
            result.RouteName.Should().Be("provider-review");
        }

        [Test, MoqAutoData]
        public async Task And_No_Ukprn_Then_Redirects_To_Employer_Route(
            ReservationsRouteModel routeModel,
            StartDateModel startDateModel,
            Course course,
            ApprenticeshipTrainingFormModel formModel,
            ReservationsController controller)
        {
            formModel.TrainingStartDate = JsonConvert.SerializeObject(startDateModel);
            formModel.CourseId = JsonConvert.SerializeObject(course);
            routeModel.Ukprn = null;

            var result = await controller.PostApprenticeshipTraining(routeModel, formModel) as RedirectToRouteResult;
            
            result.Should().NotBeNull($"result was not a {typeof(RedirectToRouteResult)}");
            result.RouteName.Should().Be("employer-review");
        }

        [Test, MoqAutoData]
        public async Task Then_Caches_Draft_Reservation(
            ReservationsRouteModel routeModel,
            StartDateModel startDateModel,
            Course course,
            ApprenticeshipTrainingFormModel formModel,
            [Frozen] Mock<IMediator> mockMediator, 
            ReservationsController controller)
        {
            formModel.TrainingStartDate = JsonConvert.SerializeObject(startDateModel);
            formModel.CourseId = JsonConvert.SerializeObject(course);

            await controller.PostApprenticeshipTraining(routeModel, formModel);

            mockMediator.Verify(mediator => 
                mediator.Send(It.Is<CacheCreateReservationCommand>(command => 
                    command.AccountPublicHashedId == routeModel.EmployerAccountId &&
                    command.StartDate == startDateModel.StartDate.ToString("yyyy-MM") &&
                    command.StartDateDescription == startDateModel.ToString() &&
                    command.CourseId == course.Id &&
                    command.CourseDescription == $"{course.Title} - Level: {course.Level}"
                    ), It.IsAny<CancellationToken>()));
        }

        [Test, MoqAutoData]
        public async Task And_No_Course_Then_Caches_Draft_Reservation(
            ReservationsRouteModel routeModel,
            StartDateModel startDateModel,
            ApprenticeshipTrainingFormModel formModel,
            [Frozen] Mock<IMediator> mockMediator, 
            ReservationsController controller)
        {
            formModel.TrainingStartDate = JsonConvert.SerializeObject(startDateModel);
            formModel.CourseId = null;

            await controller.PostApprenticeshipTraining(routeModel, formModel);

            mockMediator.Verify(mediator => 
                mediator.Send(It.Is<CacheCreateReservationCommand>(command => 
                    command.AccountPublicHashedId == routeModel.EmployerAccountId &&
                    command.StartDate == startDateModel.StartDate.ToString("yyyy-MM") &&
                    command.StartDateDescription == startDateModel.ToString() &&
                    command.CourseId == null &&
                    command.CourseDescription == "Unknown"
                ), It.IsAny<CancellationToken>()));
        }

        [Test, MoqAutoData]
        public async Task Then_Adds_Guid_To_RouteModel(
            ReservationsRouteModel routeModel,
            StartDateModel startDateModel,
            Course course,
            ApprenticeshipTrainingFormModel formModel,
            CacheReservationResult cacheReservationResult,
            [Frozen] Mock<IMediator> mockMediator, 
            ReservationsController controller)
        {
            formModel.TrainingStartDate = JsonConvert.SerializeObject(startDateModel);
            formModel.CourseId = JsonConvert.SerializeObject(course);
            mockMediator
                .Setup(mediator => mediator.Send(It.IsAny<CacheCreateReservationCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(cacheReservationResult);

            var result = await controller.PostApprenticeshipTraining(routeModel, formModel) as RedirectToRouteResult;

            result.RouteValues.Should().ContainKey("Id")
                .WhichValue.Should().Be(cacheReservationResult.Id);
        }

        [Test, AutoData]//note cannot use moqautodata to construct controller here due to modelmetadata usage.
        public async Task And_Validation_Error_Then_Returns_Validation_Error_Details(
            ReservationsRouteModel routeModel,
            StartDateModel startDateModel,
            Course course,
            ApprenticeshipTrainingFormModel formModel,
            GetCoursesResult coursesResult,
            Mock<IMediator> mockMediator)
        {
            formModel.TrainingStartDate = JsonConvert.SerializeObject(startDateModel);
            formModel.CourseId = JsonConvert.SerializeObject(course);
            mockMediator
                .Setup(mediator => mediator.Send(It.IsAny<CacheCreateReservationCommand>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ValidationException(new ValidationResult("Failed", new List<string> { "TrainingStartDate|The TrainingStartDate field is not valid." }), null, null));
            mockMediator
                .Setup(mediator => mediator.Send(It.IsAny<GetCoursesQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(coursesResult);
            var controller = new ReservationsController(mockMediator.Object, Mock.Of<IStartDateService>());
            
            var result = await controller.PostApprenticeshipTraining(routeModel, formModel);
            
            Assert.IsNotNull(result);
            var actualViewResult = result as ViewResult;
            Assert.IsNotNull(actualViewResult);
            Assert.IsFalse(actualViewResult.ViewData.ModelState.IsValid);
            Assert.IsTrue(actualViewResult.ViewData.ModelState.ContainsKey("TrainingStartDate"));
        }
    }
}