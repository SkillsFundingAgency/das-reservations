using System.Threading;
using AutoFixture;
using AutoFixture.AutoMoq;
using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.Reservations.Commands;
using SFA.DAS.Reservations.Web.Controllers;

namespace SFA.DAS.Reservations.Web.UnitTests.Reservations
{
    [TestFixture]
    public class WhenCallingPostCreate
    {
        [Test]
        public void Then_Sends_Create_Reservation_Command()
        {
            var fixture = new Fixture().Customize(new AutoMoqCustomization {ConfigureMembers = true});
            var mockMediator = fixture.Freeze<Mock<IMediator>>();
            var controller = fixture.Create<ReservationsController>();

            controller.PostCreate();

            mockMediator.Verify(mediator => mediator.Send(It.IsAny<CreateReservationCommand>(), It.IsAny<CancellationToken>()));
        }
    }
}