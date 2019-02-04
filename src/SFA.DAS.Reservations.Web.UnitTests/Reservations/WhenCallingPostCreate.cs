using System.Threading;
using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.NUnit3;
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
            controller.RouteData.Values.Add("employerAccountId", "asd908sd");

            controller.PostCreate();

            mockMediator.Verify(mediator => mediator.Send(It.IsAny<CreateReservationCommand>(), It.IsAny<CancellationToken>()));
        }

        [Test, AutoData]
        public void Then_Sets_AccountId_On_The_Command(
            string accountId)
        {
            var fixture = new Fixture().Customize(new AutoMoqCustomization {ConfigureMembers = true});
            var mockMediator = fixture.Freeze<Mock<IMediator>>();
            var controller = fixture.Create<ReservationsController>();
            controller.RouteData.Values.Add("employerAccountId", accountId);

            controller.PostCreate();

            mockMediator.Verify(mediator => 
                mediator.Send(It.Is<CreateReservationCommand>(command => 
                    command.AccountId == accountId), It.IsAny<CancellationToken>()));
        }

        // then redirects
        // then ..
    }
}