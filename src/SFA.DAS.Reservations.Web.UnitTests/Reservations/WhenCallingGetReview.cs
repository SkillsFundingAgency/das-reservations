﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.Reservations.Queries.GetCachedReservation;
using SFA.DAS.Reservations.Web.Controllers;
using SFA.DAS.Reservations.Web.Infrastructure;
using SFA.DAS.Reservations.Web.Models;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Reservations.Web.UnitTests.Reservations
{
    [TestFixture]
    public class WhenCallingGetReview
    {
        [Test, MoqAutoData]
        public async Task Then_It_Gets_Reservation_From_Cache(
            ReservationsRouteModel routeModel,
            [Frozen] Mock<IMediator> mockMediator,
            [NoAutoProperties] ReservationsController controller)
        {
            await controller.Review(routeModel);

            mockMediator.Verify(mediator => 
                mediator.Send(It.Is<GetCachedReservationQuery>(query => 
                        query.Id == routeModel.Id
                ), It.IsAny<CancellationToken>()));
        }

        [Test, MoqAutoData]
        public async Task Then_It_Returns_The_ViewModel(
            ReservationsRouteModel routeModel,
            GetCachedReservationResult cachedReservationResult,
            [Frozen] Mock<IMediator> mockMediator,
            [NoAutoProperties] ReservationsController controller)
        {
            mockMediator
                .Setup(mediator => mediator.Send(It.IsAny<GetCachedReservationQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(cachedReservationResult);
            routeModel.FromReview = false;

            var result = await controller.Review(routeModel);

            var viewModel = result.Should().BeOfType<ViewResult>()
                .Which.Model.Should().BeOfType<ReviewViewModel>().Subject;

            viewModel.RouteModel.Should().BeEquivalentTo(routeModel);
            viewModel.TrainingDate.Should().Be(cachedReservationResult.TrainingDate);
            viewModel.CourseDescription.Should().Be(cachedReservationResult.CourseDescription);
            viewModel.AccountLegalEntityName.Should().Be(cachedReservationResult.AccountLegalEntityName);
            viewModel.AccountLegalEntityPublicHashedId.Should().Be(cachedReservationResult.AccountLegalEntityPublicHashedId);
            viewModel.RouteModel.FromReview.Should().BeTrue();
            viewModel.ApprenticeshipType.Should().Be(cachedReservationResult.ApprenticeshipType);
        }

        [Test, MoqAutoData]
        public async Task And_Has_Ukprn_Then_Uses_Provider_Route_And_View(
            ReservationsRouteModel routeModel,
            [NoAutoProperties] ReservationsController controller)
        {
            var result = await controller.Review(routeModel) as ViewResult;

            ((ReviewViewModel) result.Model).ConfirmRouteName.Should().Be(RouteNames.ProviderPostReview);
            ((ReviewViewModel) result.Model).ChangeCourseRouteName.Should().Be(RouteNames.ProviderApprenticeshipTraining);
            ((ReviewViewModel) result.Model).ChangeStartDateRouteName.Should().Be(RouteNames.ProviderApprenticeshipTraining);
            result.ViewName.Should().Be(ViewNames.ProviderReview);
        }

        [Test, MoqAutoData]
        public async Task And_No_Ukprn_Then_Uses_Employer_Route_And_View(
            ReservationsRouteModel routeModel,
            [NoAutoProperties] ReservationsController controller)
        {
            routeModel.UkPrn = null;
            var result = await controller.Review(routeModel) as ViewResult;

            ((ReviewViewModel) result.Model).ConfirmRouteName.Should().Be(RouteNames.EmployerPostReview);
            ((ReviewViewModel) result.Model).ChangeCourseRouteName.Should().Be(RouteNames.EmployerSelectCourse);
            ((ReviewViewModel) result.Model).ChangeStartDateRouteName.Should().Be(RouteNames.EmployerApprenticeshipTraining);
            result.ViewName.Should().Be(ViewNames.EmployerReview);
        }

        [Test, MoqAutoData]
        public async Task And_Validation_Error_Then_Redirects_To_500(
            ReservationsRouteModel routeModel,
            [Frozen] Mock<IMediator> mockMediator,
            [NoAutoProperties] ReservationsController controller)
        {
            mockMediator
                .Setup(mediator => mediator.Send(It.IsAny<GetCachedReservationQuery>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ValidationException(new ValidationResult("Failed", new List<string> { "Id|The Id field is not valid." }), null, null));
            
            var result = await controller.Review(routeModel);
            
            Assert.IsNotNull(result);
            var actualViewResult = result as RedirectToRouteResult;
            Assert.IsNotNull(actualViewResult);
            Assert.AreEqual(RouteNames.Error500, actualViewResult.RouteName);
        }

        [Test, MoqAutoData]
        public async Task And_Other_Error_Then_Redirects_To_500(
            ReservationsRouteModel routeModel,
            [Frozen] Mock<IMediator> mockMediator,
            [NoAutoProperties] ReservationsController controller)
        {
            mockMediator
                .Setup(mediator => mediator.Send(It.IsAny<GetCachedReservationQuery>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("foo bar"));
            
            var result = await controller.Review(routeModel);
            
            Assert.IsNotNull(result);
            var actualViewResult = result as RedirectToRouteResult;
            Assert.IsNotNull(actualViewResult);
            Assert.AreEqual(RouteNames.Error500, actualViewResult.RouteName);
        }
    }
}
