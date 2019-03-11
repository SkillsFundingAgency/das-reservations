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
    public class WhenCallingGetReview
    {
        [Test, MoqAutoData]
        public async Task Then_It_Gets_Reservation_From_Cache(
            ReservationsRouteModel routeModel,
            [Frozen] Mock<IMediator> mockMediator,
            ReservationsController controller)
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
            ReservationsController controller)
        {
            var result = await controller.Review(routeModel);
            result.Should().BeOfType<ViewResult>()
                .Which.Model.Should().BeOfType<ReservationsRouteModel>()
                .Which.Should().BeSameAs(routeModel);
        }
    }
}