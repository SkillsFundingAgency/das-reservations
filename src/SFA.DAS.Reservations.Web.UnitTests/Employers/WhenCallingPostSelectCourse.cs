using System;
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
using SFA.DAS.Reservations.Application.Reservations.Queries;
using SFA.DAS.Reservations.Application.Reservations.Queries.GetCachedReservation;
using SFA.DAS.Reservations.Application.Reservations.Queries.GetCourses;
using SFA.DAS.Reservations.Application.Reservations.Services;
using SFA.DAS.Reservations.Domain.Courses;
using SFA.DAS.Reservations.Web.Controllers;
using SFA.DAS.Reservations.Web.Models;

namespace SFA.DAS.Reservations.Web.UnitTests.Employers
{
    public class WhenCallingPostSelectCourse
    {
        [Test, MoqAutoData]
        public async Task Then_Caches_Draft_Reservation(
            ReservationsRouteModel routeModel,
            CourseViewModel course,
            [Frozen] Mock<IMediator> mockMediator, 
            GetCachedReservationResult cacheResult,
            EmployerReservationsController controller)
        {
            //Assign
            var selectedCourse = JsonConvert.SerializeObject(course);

            mockMediator.Setup(mediator => mediator.Send(
                    It.IsAny<GetCachedReservationQuery>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => cacheResult);

            //Act
            await controller.PostSelectCourse(routeModel, selectedCourse);


            //Assert
            mockMediator.Verify(mediator => 
                mediator.Send(It.Is<CacheCreateReservationCommand>(command => 
                    command.AccountId.Equals(cacheResult.AccountId) &&
                    command.AccountLegalEntityId.Equals(cacheResult.AccountLegalEntityId) &&
                    command.AccountLegalEntityName.Equals(cacheResult.AccountLegalEntityName) &&
                    command.CourseId.Equals(course.Id) &&
                    command.CourseDescription.Equals(course.Description)
                    ), It.IsAny<CancellationToken>()));
        }

        [Test, MoqAutoData]
        public void Then_Throw_Error_If_No_Reservation_Found(
            ReservationsRouteModel routeModel,
            CourseViewModel course,
            [Frozen] Mock<IMediator> mockMediator, 
            GetCachedReservationResult cacheResult,
            EmployerReservationsController controller)
        {
            mockMediator.Setup(mediator => mediator.Send(
                    It.IsAny<GetCachedReservationQuery>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => null);
            
            var selectedCourse = JsonConvert.SerializeObject(course);

            Assert.ThrowsAsync<ArgumentException>(() => controller.PostSelectCourse(routeModel, selectedCourse));
        }

        [Test, MoqAutoData]
        public async Task Then_Gets_Employer_Information_From_Cache(
            [Frozen]ReservationsRouteModel routeModel,
            CourseViewModel course,
            [Frozen] Mock<IMediator> mockMediator,
            GetCachedReservationResult cacheResult,
            EmployerReservationsController controller)
        {
            mockMediator.Setup(mediator => mediator.Send(
                    It.IsAny<GetCachedReservationQuery>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => cacheResult);
           
            var selectedCourse = JsonConvert.SerializeObject(course);

            await controller.PostSelectCourse(routeModel, selectedCourse);

            mockMediator.Verify(mediator => mediator.Send(
                It.Is<GetCachedReservationQuery>(q => q.Id.Equals(routeModel.Id)),
                It.IsAny<CancellationToken>()), Times.Once);

            mockMediator.Verify(mediator => 
                mediator.Send(It.Is<CacheCreateReservationCommand>(command => 
                    command.AccountId == cacheResult.AccountId &&
                    command.AccountLegalEntityId.Equals(cacheResult.AccountLegalEntityId) &&
                    command.AccountLegalEntityName.Equals(cacheResult.AccountLegalEntityName) &&
                    command.CourseId == course.Id &&
                    command.CourseDescription == course.Description
                ), It.IsAny<CancellationToken>()));
        }

        [Test, MoqAutoData] public async Task And_No_Course_Then_Doesnt_Caches_Draft_Reservation(
            ReservationsRouteModel routeModel,
            [Frozen] Mock<IMediator> mockMediator,
            GetCachedReservationResult cacheResult,
            EmployerReservationsController controller)
        {
            mockMediator.Setup(mediator => mediator.Send(
                    It.IsAny<GetCachedReservationQuery>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => cacheResult);

            await controller.PostSelectCourse(routeModel, null);

            mockMediator.Verify(mediator => 
                mediator.Send(It.IsAny<CacheCreateReservationCommand>(), It.IsAny<CancellationToken>()), 
                Times.Never);
        }

        [Test, MoqAutoData]
        public async Task Then_Adds_Guid_To_RouteModel(
            ReservationsRouteModel routeModel,
            CourseViewModel course,
            CacheReservationResult cacheReservationResult,
            [Frozen] Mock<IMediator> mockMediator, 
            EmployerReservationsController controller)
        {
            var selectedCourse = JsonConvert.SerializeObject(course);

            mockMediator
                .Setup(mediator => mediator.Send(It.IsAny<CacheCreateReservationCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(cacheReservationResult);

            var result = await controller.PostSelectCourse(routeModel, selectedCourse) as RedirectToRouteResult;

            Assert.IsNotNull(result);

            result.RouteValues.Should().ContainKey("Id")
                .WhichValue.Should().Be(cacheReservationResult.Id);
        }

        [Test, AutoData]//note cannot use moqautodata to construct controller here due to modelmetadata usage.
        public async Task And_Validation_Error_Then_Returns_Validation_Error_Details(
            ReservationsRouteModel routeModel,
            CourseViewModel course,
            GetCoursesResult coursesResult,
            Mock<IMediator> mockMediator,
            Mock<IHashingService> mockHashingService,
            GetCachedReservationResult cacheResult)
        {
            mockMediator.Setup(mediator => mediator.Send(
                    It.IsAny<GetCachedReservationQuery>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => cacheResult);
            
            var selectedCourse = JsonConvert.SerializeObject(course);
            
            mockMediator
                .Setup(mediator => mediator.Send(It.IsAny<CacheCreateReservationCommand>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ValidationException(new ValidationResult("Failed", new List<string> { "Course|The Course field is not valid." }), null, null));
            
            mockMediator
                .Setup(mediator => mediator.Send(It.IsAny<GetCoursesQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(coursesResult);
            
            var controller = new EmployerReservationsController(mockMediator.Object, mockHashingService.Object);
            
            var result = await controller.PostSelectCourse(routeModel, selectedCourse);
            
            Assert.IsNotNull(result);
            var actualViewResult = result as ViewResult;
            Assert.IsNotNull(actualViewResult);
            Assert.IsFalse(actualViewResult.ViewData.ModelState.IsValid);
            Assert.IsTrue(actualViewResult.ViewData.ModelState.ContainsKey("Course"));
        }
    }
}
