using System;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.Reservations.Queries.GetReservation;
using SFA.DAS.Reservations.Domain.Rules;
using SFA.DAS.Reservations.Web.Controllers;
using SFA.DAS.Reservations.Web.Infrastructure;
using SFA.DAS.Reservations.Web.Models;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Reservations.Web.UnitTests.Reservations
{
    [TestFixture]
    public class WhenCallingGetDelete
    {
        [Test, MoqAutoData]
        public void And_No_Id_Then_Throws_ArgumentException(
            ReservationsRouteModel routeModel,
            ReservationsController controller)
        {
            routeModel.Id = null;

            var act  = new Func<Task>(async ()  => await controller.Delete(routeModel));

            act.Should().Throw<ArgumentException>()
                .WithMessage("Reservation ID must be in URL.*")
                .And.ParamName.Should().Be(nameof(routeModel.Id));
        }

        [Test, MoqAutoData]
        public async Task Then_Gets_Reservation_Details(
            ReservationsRouteModel routeModel,
            [Frozen] Mock<IMediator> mockMediator,
            ReservationsController controller)
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
            ReservationsController controller)
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
            ReservationsController controller)
        {
            routeModel.UkPrn = null;

            var result = await controller.Delete(routeModel) as ViewResult;

            result.ViewName.Should().Be(ViewNames.EmployerDelete);
        }

        [Test, MoqAutoData]
        public async Task And_Has_Ukprn_Then_ViewName_Is_ProviderDelete(
            ReservationsRouteModel routeModel,
            ReservationsController controller)
        {
            var result = await controller.Delete(routeModel) as ViewResult;

            result.ViewName.Should().Be(ViewNames.ProviderDelete);
        }
    }
}