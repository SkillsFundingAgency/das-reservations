﻿using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.Reservations.Commands.CreateReservation;
using SFA.DAS.Reservations.Infrastructure.Exceptions;
using SFA.DAS.Reservations.Web.Controllers;
using SFA.DAS.Reservations.Web.Infrastructure;
using SFA.DAS.Reservations.Web.Models;
using SFA.DAS.Testing.AutoFixture;


namespace SFA.DAS.Reservations.Web.UnitTests.Reservations
{
    [TestFixture]
    public class WhenCallingPostReview
    {
        [Test, MoqAutoData]
        public async Task Then_Sends_Create_Command_With_Correct_Values_Set(
            ReservationsRouteModel routeModel, 
            CreateReservationResult createReservationResult,
            [Frozen] Mock<IMediator> mockMediator,
            ReservationsController controller)
        {
            mockMediator
                .Setup(mediator => mediator.Send(It.IsAny<CreateReservationCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(createReservationResult);
            
            await controller.PostReview(routeModel);

            mockMediator.Verify(mediator => 
                mediator.Send(It.Is<CreateReservationCommand>(command => 
                    command.Id == routeModel.Id), It.IsAny<CancellationToken>()));
        }

        [Test, MoqAutoData]
        public async Task Then_Redirects_To_The_Confirmation_Employer_View_When_No_UkPrn(
            ReservationsRouteModel routeModel, 
            CreateReservationResult createReservationResult,
            [Frozen] Mock<IMediator> mockMediator,
            ReservationsController controller)
        {
            routeModel.UkPrn = null;
            mockMediator
                .Setup(mediator => mediator.Send(It.IsAny<CreateReservationCommand>(), CancellationToken.None))
                .ReturnsAsync(createReservationResult);

            var result = await controller.PostReview(routeModel) as RedirectToRouteResult;

            result.Should().NotBeNull($"result was not a {typeof(RedirectToRouteResult)}");
            result.RouteName.Should().Be(RouteNames.EmployerCompleted);
            result.RouteValues.Should().ContainKey("id").WhichValue.Should().NotBe(Guid.Empty);
        }

        [Test, MoqAutoData]
        public async Task Then_Redirects_To_The_Confirmation_Provider_View_When_Has_UkPrn(
            ReservationsRouteModel routeModel, 
            CreateReservationResult createReservationResult,
            [Frozen] Mock<IMediator> mockMediator,
            ReservationsController controller)
        {
            mockMediator
                .Setup(mediator => mediator.Send(It.IsAny<CreateReservationCommand>(), CancellationToken.None))
                .ReturnsAsync(createReservationResult);

            var result = await controller.PostReview(routeModel) as RedirectToRouteResult;

            result.Should().NotBeNull($"result was not a {typeof(RedirectToRouteResult)}");
            result.RouteName.Should().Be(RouteNames.ProviderCompleted);
            result.RouteValues.Should().ContainKey("id").WhichValue.Should().NotBe(Guid.Empty);
            result.RouteValues.Should().ContainKey("accountLegalEntityPublicHashedId")
                .WhichValue.Should().Be(createReservationResult.AccountLegalEntityPublicHashedId);
            result.RouteValues.Should().ContainKey("cohortReference")
                .WhichValue.Should().Be(createReservationResult.CohortRef);
        }

        [Test, MoqAutoData]
        public async Task And_ValidationException_And_Has_Ukprn_Then_Redirects_To_ProviderIndex(
            ReservationsRouteModel routeModel,
            ValidationException validationException,
            [Frozen] Mock<IMediator> mockMediator,
            ReservationsController controller)
        {
            mockMediator
                .Setup(x => x.Send(It.IsAny<CreateReservationCommand>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(validationException);

            var actual = await controller.PostReview(routeModel);

            actual.Should().NotBeNull();
            var redirectToRouteResult = actual as RedirectToRouteResult;
            redirectToRouteResult.Should().NotBeNull();
            redirectToRouteResult?.RouteName.Should().Be(RouteNames.ProviderIndex);
        }

        [Test, MoqAutoData]
        public async Task And_ValidationException_And_No_Ukprn_Then_Redirects_To_EmployerIndex(
            ReservationsRouteModel routeModel,
            ValidationException validationException,
            [Frozen] Mock<IMediator> mockMediator,
            ReservationsController controller)
        {
            routeModel.UkPrn = null;
            mockMediator
                .Setup(x => x.Send(It.IsAny<CreateReservationCommand>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(validationException);

            var actual = await controller.PostReview(routeModel);

            actual.Should().NotBeNull();
            var redirectToRouteResult = actual as RedirectToRouteResult;
            redirectToRouteResult.Should().NotBeNull();
            redirectToRouteResult?.RouteName.Should().Be(RouteNames.EmployerIndex);
        }

        [Test, MoqAutoData]
        public async Task And_CachedReservationNotFoundException_And_Has_Ukprn_Then_Redirects_To_ProviderIndex(
            ReservationsRouteModel routeModel,
            CachedReservationNotFoundException notFoundException,
            [Frozen] Mock<IMediator> mockMediator,
            ReservationsController controller)
        {
            mockMediator.Setup(x => x.Send(It.IsAny<CreateReservationCommand>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(notFoundException);

            var actual = await controller.PostReview(routeModel);

            actual.Should().NotBeNull();
            var redirectToRouteResult = actual as RedirectToRouteResult;
            redirectToRouteResult.Should().NotBeNull();
            redirectToRouteResult?.RouteName.Should().Be(RouteNames.ProviderIndex);
        }

        [Test, MoqAutoData]
        public async Task And_CachedReservationNotFoundException_And_No_Ukprn_Then_Redirects_To_EmployerIndex(
            ReservationsRouteModel routeModel,
            CachedReservationNotFoundException notFoundException,
            [Frozen] Mock<IMediator> mockMediator,
            ReservationsController controller)
        {
            routeModel.UkPrn = null;
            mockMediator.Setup(x => x.Send(It.IsAny<CreateReservationCommand>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(notFoundException);

            var actual = await controller.PostReview(routeModel);

            actual.Should().NotBeNull();
            var redirectToRouteResult = actual as RedirectToRouteResult;
            redirectToRouteResult.Should().NotBeNull();
            redirectToRouteResult?.RouteName.Should().Be(RouteNames.EmployerIndex);
        }
    }
}