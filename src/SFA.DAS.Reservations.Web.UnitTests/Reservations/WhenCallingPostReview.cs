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
using NUnit.Framework;
using SFA.DAS.Reservations.Application.Exceptions;
using SFA.DAS.Reservations.Application.Reservations.Commands;
using SFA.DAS.Reservations.Application.Reservations.Commands.CreateReservation;
using SFA.DAS.Reservations.Application.Reservations.Queries;
using SFA.DAS.Reservations.Application.Reservations.Queries.GetCachedReservation;
using SFA.DAS.Reservations.Application.Reservations.Queries.GetCourses;
using SFA.DAS.Reservations.Domain.Courses;
using SFA.DAS.Reservations.Infrastructure.Configuration;
using SFA.DAS.Reservations.Web.Controllers;
using SFA.DAS.Reservations.Web.Infrastructure;
using SFA.DAS.Reservations.Web.Models;
using SFA.DAS.Reservations.Web.Services;

namespace SFA.DAS.Reservations.Web.UnitTests.Reservations
{
    [TestFixture]
    public class WhenCallingPostReview
    {
        private IFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization { ConfigureMembers = true });
        }

        [Test, AutoData]
        public async Task Then_Sends_Create_Command_With_Correct_Values_Set(
            ReservationsRouteModel routeModel, 
            CreateReservationResult createReservationResult)
        {
            var mockMediator = _fixture.Freeze<Mock<IMediator>>();
            
            mockMediator
                .Setup(mediator => mediator.Send(It.IsAny<CreateReservationCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(createReservationResult);
            var controller = _fixture.Create<ReservationsController>();
            
            await controller.PostReview(routeModel);

            mockMediator.Verify(mediator => 
                mediator.Send(It.Is<CreateReservationCommand>(command => 
                    command.Id == routeModel.Id), It.IsAny<CancellationToken>()));
        }

        [Test, AutoData]
        public async Task Then_Redirects_To_The_Confirmation_Employer_View_When_No_UkPrn(
            ReservationsRouteModel routeModel, 
            CreateReservationResult createReservationResult)
        {
            routeModel.UkPrn = null;
            var mockMediator = _fixture.Freeze<Mock<IMediator>>();
            mockMediator
                .Setup(mediator => mediator.Send(It.IsAny<CreateReservationCommand>(), CancellationToken.None))
                .ReturnsAsync(createReservationResult);
            var controller = _fixture.Create<ReservationsController>();

            var result = await controller.PostReview(routeModel) as RedirectToRouteResult;

            result.Should().NotBeNull($"result was not a {typeof(RedirectToRouteResult)}");
            result.RouteName.Should().Be(RouteNames.EmployerCompleted);
            result.RouteValues.Should().ContainKey("id").WhichValue.Should().NotBe(Guid.Empty);
        }

        [Test, AutoData]
        public async Task Then_Redirects_To_The_Confirmation_Provider_View_When_Has_UkPrn(
            ReservationsRouteModel routeModel, 
            CreateReservationResult createReservationResult)
        {
            var mockMediator = _fixture.Freeze<Mock<IMediator>>();
            mockMediator
                .Setup(mediator => mediator.Send(It.IsAny<CreateReservationCommand>(), CancellationToken.None))
                .ReturnsAsync(createReservationResult);
            var controller = _fixture.Create<ReservationsController>();

            var result = await controller.PostReview(routeModel) as RedirectToRouteResult;

            result.Should().NotBeNull($"result was not a {typeof(RedirectToRouteResult)}");
            result.RouteName.Should().Be(RouteNames.ProviderCompleted);
            result.RouteValues.Should().ContainKey("id").WhichValue.Should().NotBe(Guid.Empty);
            result.RouteValues.Should().ContainKey("accountLegalEntityPublicHashedId").WhichValue.Should().Be(createReservationResult.Reservation.AccountLegalEntityPublicHashedId);
        }

        [Test, AutoData]
        public async Task And_ValidationException_Then_Redirects_To_Apprenticeship_Training(
            ReservationsRouteModel routeModel)
        {
            var mockMediator = new Mock<IMediator>();
            mockMediator.Setup(x => x.Send(It.IsAny<CreateReservationCommand>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ValidationException(new ValidationResult("Failed", new List<string> { "TrainingStartDate|The TrainingStartDate field is not valid." }), null, null));
            mockMediator.Setup(x => x.Send(It.IsAny<GetCoursesQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetCoursesResult{Courses = new List<Course>()});
            
            var controller = new ReservationsController(mockMediator.Object, Mock.Of<IStartDateService>(), Mock.Of<IOptions<ReservationsWebConfiguration>>(), Mock.Of<ILogger<ReservationsController>>());

            var actual = await controller.PostReview(routeModel);

            actual.Should().NotBeNull();
            var actualViewResult = actual as ViewResult;
            actualViewResult.Should().NotBeNull();
            actualViewResult?.ViewData.ModelState.IsValid.Should().BeFalse();
            actualViewResult?.ViewData.ModelState.Should().Contain(pair => pair.Key == "TrainingStartDate");
            actualViewResult?.ViewName.Should().Be("ApprenticeshipTraining");
        }

        [Test, AutoData]
        public async Task And_CachedReservationNotFoundException_Then_Redirects_To_Choose_Employer_Account(
            ReservationsRouteModel routeModel)
        {
            var mockMediator = new Mock<IMediator>();
            mockMediator.Setup(x => x.Send(It.IsAny<CreateReservationCommand>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new CachedReservationNotFoundException(routeModel.Id.Value));
            mockMediator.Setup(x => x.Send(It.IsAny<GetCoursesQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetCoursesResult{Courses = new List<Course>()});
            
            var controller = new ReservationsController(mockMediator.Object, Mock.Of<IStartDateService>(), Mock.Of<IOptions<ReservationsWebConfiguration>>(), Mock.Of<ILogger<ReservationsController>>());

            var actual = await controller.PostReview(routeModel);

            actual.Should().NotBeNull();
            var actualViewResult = actual as ViewResult;
            actualViewResult.Should().NotBeNull();
            actualViewResult?.ViewName.Should().Be("ApprenticeshipTraining");//todo: not this view!!! should go to beginning of caching.
        }
    }
}