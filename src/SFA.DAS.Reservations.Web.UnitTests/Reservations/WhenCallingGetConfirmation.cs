using System;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.Reservations.Queries;
using SFA.DAS.Reservations.Web.Controllers;
using SFA.DAS.Reservations.Web.Models;

namespace SFA.DAS.Reservations.Web.UnitTests.Reservations
{
    [TestFixture]
    public class WhenCallingGetConfirmation
    {
        [Test, MoqAutoData]
        public async Task Then_It_Calls_Mediator_To_Get_Reservation(
            ReservationsRouteModel routeModel,
            [Frozen] Mock<IMediator> mockMediator,
            ReservationsController controller)
        {
            await controller.Confirmation(routeModel);

            mockMediator.Verify(mediator => mediator.Send(It.Is<GetReservationQuery>(query => query.Id == routeModel.Id), CancellationToken.None));
        }

        [Test, MoqAutoData]
        public async Task Then_It_Returns_The_ViewModel(
            ReservationsRouteModel routeModel,
            GetReservationResult mediatorResult,
            [Frozen] Mock<IMediator> mockMediator,
            ReservationsController controller)
        {
            mockMediator
                .Setup(mediator => mediator.Send(It.IsAny<GetReservationQuery>(), CancellationToken.None))
                .ReturnsAsync(mediatorResult);

            var result = await controller.Confirmation(routeModel);

            var model = result.Should().BeOfType<ViewResult>()
                .Which.Model.Should().BeOfType<ConfirmationViewModel>().Subject;

            model.ReservationId.Should().Be(mediatorResult.ReservationId);
            model.StartDate.Should().Be(mediatorResult.StartDate);
            model.ExpiryDate.Should().Be(mediatorResult.ExpiryDate);
        }
    }
}