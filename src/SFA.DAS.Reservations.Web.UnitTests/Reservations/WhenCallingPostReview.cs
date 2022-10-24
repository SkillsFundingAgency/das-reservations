﻿using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.Reservations.Commands.CreateReservation;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Infrastructure.Exceptions;
using SFA.DAS.Reservations.Web.Controllers;
using SFA.DAS.Reservations.Web.Infrastructure;
using SFA.DAS.Reservations.Web.Models;
using SFA.DAS.Reservations.Web.UnitTests.Customisations;
using SFA.DAS.Testing.AutoFixture;


namespace SFA.DAS.Reservations.Web.UnitTests.Reservations
{
    [TestFixture]
    public class WhenCallingPostReview
    {
        [Test, DomainAutoData]
        public async Task And_Invalid_ViewModel_And_Has_Ukprn_Then_Continues_As_Normal(
            ReservationsRouteModel routeModel, 
            PostReviewViewModel viewModel,
            ReservationsController controller)
        {
            controller.ModelState.AddModelError("key", "error message");
            
            var result = await controller.PostReview(routeModel, viewModel) as RedirectToRouteResult;

            result.RouteName.Should().Be(RouteNames.ProviderCompleted);
        }

        [Test, DomainAutoData]
        public async Task And_Invalid_ViewModel_And_No_Ukprn_Then_Renders_Provider_Review_Again(
            ReservationsRouteModel routeModel, 
            PostReviewViewModel viewModel,
            ReservationsController controller)
        {
            routeModel.UkPrn = null;
            controller.ModelState.AddModelError("key", "error message");
            
            var result = await controller.PostReview(routeModel, viewModel) as ViewResult;

            result.ViewName.Should().Be(ViewNames.EmployerReview);
            result.Model.Should().BeEquivalentTo(new ReviewViewModel(routeModel, viewModel));
        }

        [Test, DomainAutoData]
        public async Task And_Reserve_False_And_No_Ukprn_Then_Redirect_To_Account_Home(
            ReservationsRouteModel routeModel, 
            PostReviewViewModel viewModel,
            string expectedUrl,
            [Frozen] Mock<IExternalUrlHelper> mockUrlHelper,
            ReservationsController controller)
        {
            viewModel.Reserve = false;
            routeModel.UkPrn = null;
            mockUrlHelper
                .Setup(helper => helper.GenerateDashboardUrl(routeModel.EmployerAccountId))
                .Returns(expectedUrl);

            var result = await controller.PostReview(routeModel, viewModel) as RedirectResult;

            result.Url.Should().Be(expectedUrl);
        }

        [Test, DomainAutoData]
        public async Task Then_Sends_Create_Command_With_Correct_Values_Set(
            ReservationsRouteModel routeModel, 
            PostReviewViewModel viewModel,
            [Frozen] Mock<IMediator> mockMediator,
            ReservationsController controller)
        {
            await controller.PostReview(routeModel, viewModel);

            mockMediator.Verify(mediator => 
                mediator.Send(It.Is<CreateReservationCommand>(command => 
                    command.Id == routeModel.Id), It.IsAny<CancellationToken>()));
        }


        [Test, DomainAutoData]
        public async Task Then_Sends_UserId_If_Employer_Command_With_Correct_Values_Set(
            Guid expectedUserId,
            ReservationsRouteModel routeModel,
            PostReviewViewModel viewModel,
            [Frozen] Mock<IMediator> mockMediator,
            ReservationsController controller)
        {
            routeModel.UkPrn = null;
            viewModel.Reserve = true;
            var claim = new Claim(EmployerClaims.IdamsUserIdClaimTypeIdentifier, expectedUserId.ToString());
            controller.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { claim }));
            
            await controller.PostReview(routeModel, viewModel);

            mockMediator.Verify(mediator =>
                mediator.Send(It.Is<CreateReservationCommand>(command =>
                    command.Id == routeModel.Id && command.UserId.Equals(expectedUserId)), It.IsAny<CancellationToken>()));
        }

        [Test, DomainAutoData]
        public async Task Then_Redirects_To_The_Confirmation_Employer_View_When_No_UkPrn(
            ReservationsRouteModel routeModel, 
            PostReviewViewModel viewModel,
            CreateReservationResult createReservationResult,
            [Frozen] Mock<IMediator> mockMediator,
            ReservationsController controller)
        {
            routeModel.UkPrn = null;
            var claim = new Claim(EmployerClaims.IdamsUserIdClaimTypeIdentifier, Guid.NewGuid().ToString());
            controller.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { claim }));
            viewModel.Reserve = true;
            mockMediator
                .Setup(mediator => mediator.Send(It.IsAny<CreateReservationCommand>(), CancellationToken.None))
                .ReturnsAsync(createReservationResult);

            var result = await controller.PostReview(routeModel, viewModel) as RedirectToRouteResult;

            result.Should().NotBeNull($"result was not a {typeof(RedirectToRouteResult)}");
            result.RouteName.Should().Be(RouteNames.EmployerCompleted);
            result.RouteValues.Should().ContainKey("id").WhoseValue.Should().NotBe(Guid.Empty);
        }

        [Test, DomainAutoData]
        public async Task Then_Redirects_To_The_Confirmation_Employer_View_With_ProviderId_When_Part_Of_Empty_Cohort(
            ReservationsRouteModel routeModel,
            PostReviewViewModel viewModel,
            CreateReservationResult createReservationResult,
            [Frozen] Mock<IMediator> mockMediator,
            ReservationsController controller)
        {
            createReservationResult.IsEmptyCohortFromSelect = true;
            routeModel.UkPrn = null;
            var claim = new Claim(EmployerClaims.IdamsUserIdClaimTypeIdentifier, Guid.NewGuid().ToString());
            controller.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { claim }));
            viewModel.Reserve = true;
            mockMediator
                .Setup(mediator => mediator.Send(It.IsAny<CreateReservationCommand>(), CancellationToken.None))
                .ReturnsAsync(createReservationResult);

            var result = await controller.PostReview(routeModel, viewModel) as RedirectToRouteResult;

            result.Should().NotBeNull($"result was not a {typeof(RedirectToRouteResult)}");
            result.RouteName.Should().Be(RouteNames.EmployerCompleted);
            result.RouteValues.Should().ContainKey("id").WhoseValue.Should().NotBe(Guid.Empty);
            result.RouteValues.Should().ContainKey("cohortReference")
                .WhoseValue.Should().Be(createReservationResult.CohortRef);
            result.RouteValues.Should().ContainKey("accountLegalEntityPublicHashedId")
                .WhoseValue.Should().Be(createReservationResult.AccountLegalEntityPublicHashedId);
            result.RouteValues.Should().ContainKey("providerId")
                .WhoseValue.Should().Be(createReservationResult.ProviderId);
        }

        [Test, DomainAutoData]
        public async Task Then_Redirects_To_The_Confirmation_Employer_View_With_Journey_Data(
            ReservationsRouteModel routeModel,
            PostReviewViewModel viewModel,
            CreateReservationResult createReservationResult,
            [Frozen] Mock<IMediator> mockMediator,
            ReservationsController controller)
        {
            createReservationResult.IsEmptyCohortFromSelect = true;
            routeModel.UkPrn = null;
            var claim = new Claim(EmployerClaims.IdamsUserIdClaimTypeIdentifier, Guid.NewGuid().ToString());
            controller.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { claim }));
            viewModel.Reserve = true;
            mockMediator
                .Setup(mediator => mediator.Send(It.IsAny<CreateReservationCommand>(), CancellationToken.None))
                .ReturnsAsync(createReservationResult);

            var result = await controller.PostReview(routeModel, viewModel) as RedirectToRouteResult;

            result.Should().NotBeNull($"result was not a {typeof(RedirectToRouteResult)}");
            result.RouteName.Should().Be(RouteNames.EmployerCompleted);
            result.RouteValues.Should().ContainKey("journeyData")
                .WhoseValue.Should().Be(createReservationResult.JourneyData);
        }

        [Test, DomainAutoData]
        public async Task Then_Redirects_To_The_Confirmation_Provider_View_When_Has_UkPrn(
            ReservationsRouteModel routeModel, 
            PostReviewViewModel viewModel,
            CreateReservationResult createReservationResult,
            [Frozen] Mock<IMediator> mockMediator,
            ReservationsController controller)
        {
            mockMediator
                .Setup(mediator => mediator.Send(It.IsAny<CreateReservationCommand>(), CancellationToken.None))
                .ReturnsAsync(createReservationResult);

            var result = await controller.PostReview(routeModel, viewModel) as RedirectToRouteResult;

            result.Should().NotBeNull($"result was not a {typeof(RedirectToRouteResult)}");
            result.RouteName.Should().Be(RouteNames.ProviderCompleted);
            result.RouteValues.Should().ContainKey("id").WhoseValue.Should().NotBe(Guid.Empty);
            result.RouteValues.Should().ContainKey("accountLegalEntityPublicHashedId")
                .WhoseValue.Should().Be(createReservationResult.AccountLegalEntityPublicHashedId);
            result.RouteValues.Should().ContainKey("cohortReference")
                .WhoseValue.Should().Be(createReservationResult.CohortRef);
        }

        [Test, DomainAutoData]
        public async Task And_ValidationException_And_Has_Ukprn_Then_Redirects_To_ProviderIndex(
            ReservationsRouteModel routeModel,
            PostReviewViewModel viewModel,
            ValidationException validationException,
            [Frozen] Mock<IMediator> mockMediator,
            ReservationsController controller)
        {
            mockMediator
                .Setup(x => x.Send(It.IsAny<CreateReservationCommand>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(validationException);

            var actual = await controller.PostReview(routeModel, viewModel);

            actual.Should().NotBeNull();
            var redirectToRouteResult = actual as RedirectToRouteResult;
            redirectToRouteResult.Should().NotBeNull();
            redirectToRouteResult?.RouteName.Should().Be(RouteNames.ProviderIndex);
        }

        [Test, DomainAutoData]
        public async Task And_ValidationException_And_No_Ukprn_Then_Redirects_To_EmployerIndex(
            ReservationsRouteModel routeModel,
            PostReviewViewModel viewModel,
            ValidationException validationException,
            [Frozen] Mock<IMediator> mockMediator,
            ReservationsController controller)
        {
            routeModel.UkPrn = null;
            var claim = new Claim(EmployerClaims.IdamsUserIdClaimTypeIdentifier, Guid.NewGuid().ToString());
            controller.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { claim }));
            viewModel.Reserve = true;
            mockMediator
                .Setup(x => x.Send(It.IsAny<CreateReservationCommand>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(validationException);

            var actual = await controller.PostReview(routeModel, viewModel);

            actual.Should().NotBeNull();
            var redirectToRouteResult = actual as RedirectToRouteResult;
            redirectToRouteResult.Should().NotBeNull();
            redirectToRouteResult?.RouteName.Should().Be(RouteNames.EmployerIndex);
        }

        [Test, DomainAutoData]
        public async Task And_CachedReservationNotFoundException_And_Has_Ukprn_Then_Redirects_To_ProviderIndex(
            ReservationsRouteModel routeModel,
            PostReviewViewModel viewModel,
            CachedReservationNotFoundException notFoundException,
            [Frozen] Mock<IMediator> mockMediator,
            ReservationsController controller)
        {
            mockMediator.Setup(x => x.Send(It.IsAny<CreateReservationCommand>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(notFoundException);

            var actual = await controller.PostReview(routeModel, viewModel);

            actual.Should().NotBeNull();
            var redirectToRouteResult = actual as RedirectToRouteResult;
            redirectToRouteResult.Should().NotBeNull();
            redirectToRouteResult?.RouteName.Should().Be(RouteNames.ProviderIndex);
        }

        [Test, DomainAutoData]
        public async Task And_CachedReservationNotFoundException_And_No_Ukprn_Then_Redirects_To_EmployerIndex(
            ReservationsRouteModel routeModel,
            PostReviewViewModel viewModel,
            CachedReservationNotFoundException notFoundException,
            [Frozen] Mock<IMediator> mockMediator,
            ReservationsController controller)
        {
            routeModel.UkPrn = null;
            var claim = new Claim(EmployerClaims.IdamsUserIdClaimTypeIdentifier, Guid.NewGuid().ToString());
            controller.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { claim }));
            viewModel.Reserve = true;
            mockMediator.Setup(x => x.Send(It.IsAny<CreateReservationCommand>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(notFoundException);

            var actual = await controller.PostReview(routeModel, viewModel);

            actual.Should().NotBeNull();
            var redirectToRouteResult = actual as RedirectToRouteResult;
            redirectToRouteResult.Should().NotBeNull();
            redirectToRouteResult?.RouteName.Should().Be(RouteNames.EmployerIndex);
        }
    }
}