using System;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.NUnit3;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
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
        public async Task Then_Sends_Create_Reservation_Command()
        {
            var fixture = new Fixture().Customize(new AutoMoqCustomization {ConfigureMembers = true});
            var mockMediator = fixture.Freeze<Mock<IMediator>>();
            var controller = fixture.Create<ReservationsController>();
            controller.RouteData.Values.Add("employerAccountId", "asd908sd");

            await controller.PostCreate();

            mockMediator.Verify(mediator => mediator.Send(It.IsAny<CreateReservationCommand>(), It.IsAny<CancellationToken>()));
        }

        [Test, AutoData]
        public async Task Then_Sets_The_Correct_Values_On_The_Command(
            string accountId)
        {
            var fixture = new Fixture().Customize(new AutoMoqCustomization {ConfigureMembers = true});
            var mockMediator = fixture.Freeze<Mock<IMediator>>();
            var controller = fixture.Create<ReservationsController>();
            controller.RouteData.Values.Add("employerAccountId", accountId);

            await controller.PostCreate();

            mockMediator.Verify(mediator => 
                mediator.Send(It.Is<CreateReservationCommand>(command => 
                    command.AccountId == accountId && 
                    command.StartDate == DateTime.Today
                        ), It.IsAny<CancellationToken>()));
        }

        [Test, AutoData]
        public async Task Then_Redirects_To_The_Confirmation_View(
            string accountId)
        {
            var fixture = new Fixture().Customize(new AutoMoqCustomization {ConfigureMembers = true});
            var controller = fixture.Create<ReservationsController>();
            controller.RouteData.Values.Add("employerAccountId", accountId);

            var result = await controller.PostCreate() as RedirectToActionResult;

            result.Should().NotBeNull($"result was not a {typeof(RedirectToActionResult)}");
            result.ActionName.Should().Be(nameof(ReservationsController.Confirmation));
        }
    }
}