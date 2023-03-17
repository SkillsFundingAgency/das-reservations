using System;
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
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Web.Controllers;
using SFA.DAS.Reservations.Web.Infrastructure;
using SFA.DAS.Reservations.Web.Models;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Reservations.Web.UnitTests.Employers
{
    public class WhenViewingAvailableCourses
    {
        [Test, MoqAutoData]
        public async Task Then_Gets_Cached_Reservation(
            ReservationsRouteModel routeModel,
            GetCoursesResult coursesResult,
            [Frozen] Mock<IMediator> mockMediator,
            [NoAutoProperties] EmployerReservationsController controller)
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
            [NoAutoProperties] EmployerReservationsController controller)
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
            [NoAutoProperties] EmployerReservationsController controller,
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
            [NoAutoProperties] EmployerReservationsController controller)
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
            [NoAutoProperties] EmployerReservationsController controller,
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

        [Test, MoqAutoData]
        public async Task Then_The_BackLink_Is_Set_To_Return_To_CohortDetails_If_There_Is_A_Cohort_Ref(
            ICollection<Course> courses,
            [Frozen] Mock<IExternalUrlHelper> externalUrlHelper,
            [Frozen] Mock<IMediator> mockMediator,
            ReservationsRouteModel routeModel,
            GetCachedReservationResult cachedReservationResult,
            string cohortUrl,
            [NoAutoProperties] EmployerReservationsController controller
            )
        {
            //Arrange
            cachedReservationResult.CohortRef = "ABC123";
            cachedReservationResult.IsEmptyCohortFromSelect = false;
            cachedReservationResult.UkPrn = null;
            mockMediator.Setup(m => m.Send(It.IsAny<GetCoursesQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetCoursesResult
                {
                    Courses = courses
                });
            mockMediator
                .Setup(mediator => mediator.Send(It.IsAny<GetCachedReservationQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(cachedReservationResult);
            externalUrlHelper.Setup(x => 
                x.GenerateCohortDetailsUrl(
                    null, routeModel.EmployerAccountId, cachedReservationResult.CohortRef, false,
                    It.IsAny<string>(), cachedReservationResult.AccountLegalEntityPublicHashedId
                    ))
                .Returns(cohortUrl);

            //Act
            var result = await controller.SelectCourse(routeModel) as ViewResult;

            //Assert
            var viewModel = result?.Model as EmployerSelectCourseViewModel;
            Assert.IsNotNull(viewModel);
            Assert.AreEqual(cohortUrl,viewModel.BackLink);
            Assert.AreEqual(cachedReservationResult.CohortRef,viewModel.CohortReference);
            Assert.AreEqual(cachedReservationResult.IsEmptyCohortFromSelect,viewModel.IsEmptyCohortFromSelect);
        }


        [Test, MoqAutoData]
        public async Task Then_The_BackLink_Is_Set_To_Return_To_Empty_CohortDetails_If_Part_Of_Empty_Cohort_Journey(
            ICollection<Course> courses,
            [Frozen] Mock<IExternalUrlHelper> externalUrlHelper,
            [Frozen] Mock<IMediator> mockMediator,
            ReservationsRouteModel routeModel,
            GetCachedReservationResult cachedReservationResult,
            string cohortUrl,
            [NoAutoProperties] EmployerReservationsController controller
        )
        {
            //Arrange
            cachedReservationResult.IsEmptyCohortFromSelect = true;
            cachedReservationResult.CohortRef = "";
            routeModel.CohortReference = "";
            mockMediator.Setup(m => m.Send(It.IsAny<GetCoursesQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetCoursesResult
                {
                    Courses = courses
                });
            mockMediator
                .Setup(mediator => mediator.Send(It.IsAny<GetCachedReservationQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(cachedReservationResult);
            externalUrlHelper.Setup(x =>
                    x.GenerateCohortDetailsUrl(
                        cachedReservationResult.UkPrn, routeModel.EmployerAccountId, string.Empty, 
                        true, It.IsAny<string>(), cachedReservationResult.AccountLegalEntityPublicHashedId
                    ))
                .Returns(cohortUrl);

            //Act
            var result = await controller.SelectCourse(routeModel) as ViewResult;

            //Assert
            var viewModel = result?.Model as EmployerSelectCourseViewModel;
            Assert.IsNotNull(viewModel);
            Assert.AreEqual(cohortUrl, viewModel.BackLink);
            Assert.AreEqual(cachedReservationResult.CohortRef, viewModel.CohortReference);
        }

        [Test, MoqAutoData]
        public async Task Then_The_BackLink_Is_Set_To_Return_To_ReviewPage_If_There_Is_FromReview_Flag(
            ICollection<Course> courses,
            [Frozen] Mock<IMediator> mockMediator,
            ReservationsRouteModel routeModel,
            [NoAutoProperties] EmployerReservationsController controller)
        {
            //Arrange
            routeModel.FromReview = true;
            routeModel.CohortReference = string.Empty;
            mockMediator
                .Setup(m => m.Send(It.IsAny<GetCoursesQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetCoursesResult { Courses = courses });

            //Act
            var result = await controller.SelectCourse(routeModel) as ViewResult;

            //Assert
            var viewModel = result?.Model as EmployerSelectCourseViewModel;
            Assert.IsNotNull(viewModel);
            Assert.AreEqual(RouteNames.EmployerReview, viewModel.BackLink);
        }


        [Test, MoqAutoData]
        public async Task Then_The_BackLink_Is_Set_To_Return_To_SelectLegalEntityView(
            ReservationsRouteModel routeModel,
            ICollection<Course> courses,
            GetCachedReservationResult cachedReservationResult,
            [Frozen] Mock<IMediator> mockMediator,
            [NoAutoProperties] EmployerReservationsController controller)
        {
            //Arrange
            routeModel.FromReview = false;
            routeModel.IsFromManage = null;
            cachedReservationResult.CohortRef = string.Empty;
            cachedReservationResult.EmployerHasSingleLegalEntity = false;
            cachedReservationResult.IsEmptyCohortFromSelect = false;
            mockMediator
                .Setup(m => m.Send(It.IsAny<GetCoursesQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetCoursesResult { Courses = courses });
            mockMediator
                .Setup(mediator => mediator.Send(It.IsAny<GetCachedReservationQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(cachedReservationResult);

            //Act
            var result = await controller.SelectCourse(routeModel) as ViewResult;

            //Assert
            var viewModel = result?.Model as EmployerSelectCourseViewModel;
            Assert.IsNotNull(viewModel);
            Assert.AreEqual(RouteNames.EmployerSelectLegalEntity, viewModel.BackLink);
        }

        [Test, MoqAutoData]
        public async Task And_Single_Legal_Entity_Then_The_BackLink_Is_Set_To_Return_To_Start(
            ReservationsRouteModel routeModel,
            ICollection<Course> courses,
            GetCachedReservationResult cachedReservationResult,
            [Frozen] Mock<IMediator> mockMediator,
            [NoAutoProperties] EmployerReservationsController controller)
        {
            //Arrange
            routeModel.FromReview = false;
            cachedReservationResult.CohortRef = string.Empty;
            cachedReservationResult.IsEmptyCohortFromSelect = false;
            cachedReservationResult.EmployerHasSingleLegalEntity = true;
            mockMediator
                .Setup(m => m.Send(It.IsAny<GetCoursesQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetCoursesResult { Courses = courses });
            mockMediator
                .Setup(mediator => mediator.Send(It.IsAny<GetCachedReservationQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(cachedReservationResult);

            //Act
            var result = await controller.SelectCourse(routeModel) as ViewResult;

            //Assert
            var viewModel = result?.Model as EmployerSelectCourseViewModel;
            Assert.IsNotNull(viewModel);
            Assert.AreEqual(RouteNames.EmployerStart, viewModel.BackLink);
        }
    }
}
