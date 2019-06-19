using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.Reservations.Queries.GetReservations;
using SFA.DAS.Reservations.Web.Controllers;
using SFA.DAS.Reservations.Web.Models;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Reservations.Web.UnitTests.Reservations
{
    [TestFixture]
    public class WhenCallingGetSelectReservation
    {
        [Test, MoqAutoData]
        public async Task Then_Gets_Reservations_For_Employer_Account(
            ReservationsRouteModel routeModel,
            SelectReservationModel model,
            [Frozen] Mock<IMediator> mockMediator,
            ReservationsController controller)
        {
            controller.SelectReservation(routeModel, model);

            mockMediator.Verify(mediator => mediator.Send(It.IsAny<GetReservationsQuery>(), It.IsAny<CancellationToken>()));
        }
    }
}