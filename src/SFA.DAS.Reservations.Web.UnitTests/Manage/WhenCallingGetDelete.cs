using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.Reservations.Queries.GetReservation;
using SFA.DAS.Reservations.Web.Controllers;
using SFA.DAS.Reservations.Web.Infrastructure;
using SFA.DAS.Reservations.Web.Models;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Reservations.Web.UnitTests.Manage
{
    [TestFixture]
    public class WhenCallingGetDelete
    {
        [Test, MoqAutoData]
        public async Task And_Has_Ukprn_And_No_Id_Then_Redirects_To_Provider_Manage(
            ReservationsRouteModel routeModel,
            [NoAutoProperties] ManageReservationsController controller)
        {
            routeModel.Id = null;

            var result  = await controller.Delete(routeModel) as RedirectToRouteResult;

            result.Should().NotBeNull();
            result.RouteName.Should().Be(RouteNames.ProviderManage);
        }

        [Test, MoqAutoData]
        public async Task And_No_Ukprn_And_No_Id_Then_Redirects_To_Employer_Manage(
            ReservationsRouteModel routeModel,
            [NoAutoProperties] ManageReservationsController controller)
        {
            routeModel.UkPrn = null;
            routeModel.Id = null;

            var result  = await controller.Delete(routeModel) as RedirectToRouteResult;

            result.Should().NotBeNull();
            result.RouteName.Should().Be(RouteNames.EmployerManage);
        }

        [Test, MoqAutoData]
        public async Task Then_Gets_Reservation_Details(
            ReservationsRouteModel routeModel,
            [Frozen] Mock<IMediator> mockMediator,
            [NoAutoProperties] ManageReservationsController controller)
        {
            await controller.Delete(routeModel);

            mockMediator.Verify(mediator => mediator.Send(
                It.Is<GetReservationQuery>(query => query.Id == routeModel.Id),
                CancellationToken.None), 
                Times.Once);
        }

        [Test, MoqAutoData]
        public async Task Then_Sets_Reservation_Details_On_ViewModel(
            ReservationsRouteModel routeModel,
            GetReservationResult getReservationResult,
            [Frozen] Mock<IMediator> mockMediator,
            [NoAutoProperties] ManageReservationsController controller)
        {
            mockMediator
                .Setup(mediator => mediator.Send(
                    It.IsAny<GetReservationQuery>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(getReservationResult);

            var result = await controller.Delete(routeModel);

            var viewModel = result.Should().BeOfType<ViewResult>()
                .Which.Model.Should().BeOfType<DeleteViewModel>().Subject;

            viewModel.Should().BeEquivalentTo(new DeleteViewModel(getReservationResult));
        }

        [Test, MoqAutoData]
        public async Task And_No_Ukprn_Then_ViewName_Is_EmployerDelete(
            ReservationsRouteModel routeModel,
            [NoAutoProperties] ManageReservationsController controller)
        {
            routeModel.UkPrn = null;

            var result = await controller.Delete(routeModel) as ViewResult;

            result.ViewName.Should().Be(ViewNames.EmployerDelete);
        }

        [Test, MoqAutoData]
        public async Task And_Has_Ukprn_Then_ViewName_Is_ProviderDelete(
            ReservationsRouteModel routeModel,
            [NoAutoProperties] ManageReservationsController controller)
        {
            var result = await controller.Delete(routeModel) as ViewResult;

            result.ViewName.Should().Be(ViewNames.ProviderDelete);
        }
    }
}