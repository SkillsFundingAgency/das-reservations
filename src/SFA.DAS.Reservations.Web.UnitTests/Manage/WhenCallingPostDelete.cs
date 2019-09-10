using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.Reservations.Commands.DeleteReservation;
using SFA.DAS.Reservations.Web.Controllers;
using SFA.DAS.Reservations.Web.Infrastructure;
using SFA.DAS.Reservations.Web.Models;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Reservations.Web.UnitTests.Manage
{
    [TestFixture]
    public class WhenCallingPostDelete
    {
        [Test, MoqAutoData]
        public async Task And_Delete_Invalid_And_Has_Ukprn_Then_Shows_Provider_Delete_View_Again(
            ReservationsRouteModel routeModel,
            DeleteViewModel viewModel,
            ManageReservationsController controller)
        {
            controller.ModelState.AddModelError("key", "error message");

            var result = await controller.PostDelete(routeModel, viewModel) as ViewResult;

            result.ViewName.Should().Be(ViewNames.ProviderDelete);
            result.Model.Should().Be(viewModel);
        }

        [Test, MoqAutoData]
        public async Task And_Delete_Invalid_And_No_Ukprn_Then_Shows_Employer_Delete_View_Again(
            ReservationsRouteModel routeModel,
            DeleteViewModel viewModel,
            ManageReservationsController controller)
        {
            routeModel.UkPrn = null;
            controller.ModelState.AddModelError("key", "error message");

            var result = await controller.PostDelete(routeModel, viewModel) as ViewResult;

            result.ViewName.Should().Be(ViewNames.EmployerDelete);
            result.Model.Should().Be(viewModel);
        }

        [Test, MoqAutoData]
        public async Task And_Has_Ukprn_And_No_Id_Then_Redirect_To_Provider_Manage(
            ReservationsRouteModel routeModel,
            DeleteViewModel viewModel,
            ManageReservationsController controller)
        {
            viewModel.Delete = true;
            routeModel.Id = null;

            var result = await controller.PostDelete(routeModel, viewModel) as RedirectToRouteResult;

            result.RouteName.Should().Be(RouteNames.ProviderManage);
        }

        [Test, MoqAutoData]
        public async Task And_No_Ukprn_And_No_Id_Then_Redirect_To_Employer_Manage(
            ReservationsRouteModel routeModel,
            DeleteViewModel viewModel,
            ManageReservationsController controller)
        {
            viewModel.Delete = true;
            routeModel.UkPrn = null;
            routeModel.Id = null;

            var result = await controller.PostDelete(routeModel, viewModel) as RedirectToRouteResult;

            result.RouteName.Should().Be(RouteNames.EmployerManage);
        }

        [Test, MoqAutoData]
        public async Task And_Has_Ukprn_And_ValidationException_Then_Returns_To_Provider_Delete_View(
            ReservationsRouteModel routeModel,
            DeleteViewModel viewModel,
            ValidationException validationException,
            [Frozen] Mock<IMediator> mockMediator,
            ManageReservationsController controller)
        {
            viewModel.Delete = true;
            mockMediator
                .Setup(mediator => mediator.Send(It.IsAny<DeleteReservationCommand>(), It.IsAny<CancellationToken>()))
                .Throws(validationException);

            var result = await controller.PostDelete(routeModel, viewModel) as ViewResult;

            result.ViewName.Should().Be(ViewNames.ProviderDelete);
        }

        [Test, MoqAutoData]
        public async Task And_No_Ukprn_And_ValidationException_Then_Returns_To_Employer_Delete_View(
            ReservationsRouteModel routeModel,
            DeleteViewModel viewModel,
            ValidationException validationException,
            [Frozen] Mock<IMediator> mockMediator,
            ManageReservationsController controller)
        {
            viewModel.Delete = true;
            routeModel.UkPrn = null;
            mockMediator
                .Setup(mediator => mediator.Send(It.IsAny<DeleteReservationCommand>(), It.IsAny<CancellationToken>()))
                .Throws(validationException);

            var result = await controller.PostDelete(routeModel, viewModel) as ViewResult;

            result.ViewName.Should().Be(ViewNames.EmployerDelete);
        }

        [Test, MoqAutoData]
        public async Task And_Has_Ukprn_And_Exception_Then_Redirects_To_Provider_Error(
            ReservationsRouteModel routeModel,
            DeleteViewModel viewModel,
            Exception exception,
            [Frozen] Mock<IMediator> mockMediator,
            ManageReservationsController controller)
        {
            viewModel.Delete = true;
            mockMediator
                .Setup(mediator => mediator.Send(It.IsAny<DeleteReservationCommand>(), It.IsAny<CancellationToken>()))
                .Throws(exception);

            var result = await controller.PostDelete(routeModel, viewModel) as RedirectToRouteResult;

            result.RouteName.Should().Be(RouteNames.ProviderError);
        }

        [Test, MoqAutoData]
        public async Task And_No_Ukprn_And_Exception_Then_Redirects_To_Employer_Error(
            ReservationsRouteModel routeModel,
            DeleteViewModel viewModel,
            Exception exception,
            [Frozen] Mock<IMediator> mockMediator,
            ManageReservationsController controller)
        {
            routeModel.UkPrn = null;
            viewModel.Delete = true;
            mockMediator
                .Setup(mediator => mediator.Send(It.IsAny<DeleteReservationCommand>(), It.IsAny<CancellationToken>()))
                .Throws(exception);

            var result = await controller.PostDelete(routeModel, viewModel) as RedirectToRouteResult;

            result.RouteName.Should().Be(RouteNames.EmployerError);
        }

        [Test, MoqAutoData]
        public async Task And_Delete_True_And_Has_Ukprn_Then_Redirects_To_Provider_Completed_Route(
            ReservationsRouteModel routeModel,
            DeleteViewModel viewModel,
            [Frozen] Mock<IMediator> mockMediator,
            ManageReservationsController controller)
        {
            viewModel.Delete = true;

            var result = await controller.PostDelete(routeModel, viewModel) as RedirectToRouteResult;

            result.RouteName.Should().Be(RouteNames.ProviderDeleteCompleted);
            mockMediator.Verify(mediator => mediator.Send(
                It.Is<DeleteReservationCommand>(command => command.ReservationId == routeModel.Id),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test, MoqAutoData]
        public async Task And_Delete_True_And_No_Ukprn_Then_Redirects_To_Employer_Completed_Route(
            ReservationsRouteModel routeModel,
            DeleteViewModel viewModel,
            [Frozen] Mock<IMediator> mockMediator,
            ManageReservationsController controller)
        {
            viewModel.Delete = true;
            routeModel.UkPrn = null;

            var result = await controller.PostDelete(routeModel, viewModel) as RedirectToRouteResult;

            result.RouteName.Should().Be(RouteNames.EmployerDeleteCompleted);
            mockMediator.Verify(mediator => mediator.Send(
                    It.Is<DeleteReservationCommand>(command => command.ReservationId == routeModel.Id),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test, MoqAutoData]
        public async Task And_Delete_False_And_Has_Ukprn_Then_Redirects_To_Provider_Manage_Route(
            ReservationsRouteModel routeModel,
            DeleteViewModel viewModel,
            ManageReservationsController controller)
        {
            var result = await controller.PostDelete(routeModel, viewModel) as RedirectToRouteResult;

            result.RouteName.Should().Be(RouteNames.ProviderManage);
        }

        [Test, MoqAutoData]
        public async Task And_Delete_False_And_No_Ukprn_Then_Redirects_To_Employer_Manage_Route(
            ReservationsRouteModel routeModel,
            DeleteViewModel viewModel,
            ManageReservationsController controller)
        {
            routeModel.UkPrn = null;

            var result = await controller.PostDelete(routeModel, viewModel) as RedirectToRouteResult;

            result.RouteName.Should().Be(RouteNames.EmployerManage);
        }
    }
}