using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.Reservations.Queries.GetCachedReservation;
using SFA.DAS.Reservations.Application.Reservations.Queries.GetCourses;
using SFA.DAS.Reservations.Domain.Courses;
using SFA.DAS.Reservations.Web.Controllers;
using SFA.DAS.Reservations.Web.Models;

namespace SFA.DAS.Reservations.Web.UnitTests.Employers
{
    public class WhenViewingAvailableCourses
    {
        [Test, MoqAutoData]
        public async Task Then_Gets_Cached_Reservation(
            ReservationsRouteModel routeModel,
            GetCoursesResult coursesResult,
            [Frozen] Mock<IMediator> mockMediator,
            EmployerReservationsController controller)
        {
            mockMediator
                .Setup(m => m.Send(It.IsAny<GetCoursesQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(coursesResult);

            await controller.SelectCourse(routeModel);

            mockMediator.Verify(mediator => mediator.Send(It.Is<GetCachedReservationQuery>(query => query.Id == routeModel.Id.Value), CancellationToken.None), Times.Once);
        }

        [Test, MoqAutoData]
        public async Task And_No_Cached_Reservation_Then_Redirects_Start_Again(
            ReservationsRouteModel routeModel,
            GetCoursesResult coursesResult,
            [Frozen] Mock<IMediator> mockMediator,
            EmployerReservationsController controller)
        {
            mockMediator
                .Setup(m => m.Send(It.IsAny<GetCoursesQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(coursesResult);
            mockMediator
                .Setup(mediator => mediator.Send(It.IsAny<GetCachedReservationQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((GetCachedReservationResult)null);

            var result = await controller.SelectCourse(routeModel) as ViewResult;
            result.Should().NotBeNull();
            result?.ViewName.Should().Be("Index");
        }

        [Test, MoqAutoData]
        public async Task Then_All_Available_Courses_Are_Viewable(
            ICollection<Course> courses,
            [Frozen] Mock<IMediator> mockMediator,
            EmployerReservationsController controller,
            ReservationsRouteModel routeModel)
        {
            //Assign
            mockMediator.Setup(m => m.Send(It.IsAny<GetCoursesQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetCoursesResult
                {
                    Courses = courses
                });

            var expectedCourseViewModels = courses.Select(c => new CourseViewModel(c)).ToArray();

            //Act
            var result = await controller.SelectCourse(routeModel) as ViewResult;
            var viewModel = result?.Model as EmployerSelectCourseViewModel;

            //Assert
            Assert.IsNotNull(viewModel);
            expectedCourseViewModels.Should().BeEquivalentTo(viewModel.Courses);
        }

        [Test, MoqAutoData]
        public async Task And_Previously_Selected_Course_Then_Sets_Selected_Course(
            ReservationsRouteModel routeModel,
            GetCoursesResult coursesResult,
            GetCachedReservationResult cachedReservationResult,
            [Frozen] Mock<IMediator> mockMediator,
            EmployerReservationsController controller)
        {
            cachedReservationResult.CourseId = coursesResult.Courses.First().Id;
            mockMediator
                .Setup(m => m.Send(It.IsAny<GetCoursesQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(coursesResult);
            mockMediator
                .Setup(mediator => mediator.Send(It.IsAny<GetCachedReservationQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(cachedReservationResult);

            var result = await controller.SelectCourse(routeModel) as ViewResult;
            result.Should().NotBeNull();
            var viewModel = result.Model as EmployerSelectCourseViewModel;
            viewModel.Courses.Single(model => model.Selected == "selected").Id
                .Should().Be(cachedReservationResult.CourseId);
        }

        [Test, MoqAutoData]
        public async Task Then_ReservationId_Will_Be_In_View_Model(
            ICollection<Course> courses,
            [Frozen] Mock<IMediator> mockMediator,
            EmployerReservationsController controller,
            ReservationsRouteModel routeModel)
        {
            //Assign
         
            mockMediator.Setup(m => m.Send(It.IsAny<GetCoursesQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetCoursesResult
                {
                    Courses = courses
                });

            //Act
            var result = await controller.SelectCourse(routeModel) as ViewResult;
            var viewModel = result?.Model as EmployerSelectCourseViewModel;

            //Assert
            Assert.IsNotNull(viewModel);
            Assert.AreEqual(routeModel.Id, viewModel.ReservationId);
        }
    }
}
