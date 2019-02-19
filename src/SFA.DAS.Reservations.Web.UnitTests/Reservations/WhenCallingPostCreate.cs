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
using SFA.DAS.Reservations.Web.Models;

namespace SFA.DAS.Reservations.Web.UnitTests.Reservations
{
    [TestFixture]
    public class WhenCallingPostCreate
    {
        [Test, AutoData]
        public async Task Then_Sends_Command_With_Correct_Values_Set(
            ReservationsRouteModel routeModel)
        {
            var fixture = new Fixture().Customize(new AutoMoqCustomization {ConfigureMembers = true});
            var mockMediator = fixture.Freeze<Mock<IMediator>>();
            var controller = fixture.Create<ReservationsController>();
            controller.RouteData.Values.Add("employerAccountId", routeModel.EmployerAccountId);

            await controller.Create(routeModel.EmployerAccountId,null);

            mockMediator.Verify(mediator => 
                mediator.Send(It.Is<CreateReservationCommand>(command => 
                    command.AccountId == routeModel.EmployerAccountId && 
                    command.StartDate == DateTime.Today
                        ), It.IsAny<CancellationToken>()));
        }

        [Test, AutoData]
        public async Task Then_Redirects_To_The_Confirmation_Employer_View_When_No_UkPrn(string employerAccountId)
        {
            var fixture = new Fixture().Customize(new AutoMoqCustomization {ConfigureMembers = true});
            var controller = fixture.Create<ReservationsController>();
            controller.RouteData.Values.Add("employerAccountId", employerAccountId);

            var result = await controller.Create(employerAccountId,null) as RedirectToRouteResult;

            result.Should().NotBeNull($"result was not a {typeof(RedirectToRouteResult)}");
            result.RouteName.Should().Be("employer-reservation-created");
        }

        [Test, AutoData]
        public async Task Then_Redirects_To_The_Confirmation_Provider_View_When_Has_UkPrn(string employerAccountId, int ukPrn)
        {
            var fixture = new Fixture().Customize(new AutoMoqCustomization { ConfigureMembers = true });
            var controller = fixture.Create<ReservationsController>();
            controller.RouteData.Values.Add("employerAccountId", employerAccountId);

            var result = await controller.Create(employerAccountId, ukPrn) as RedirectToRouteResult;

            result.Should().NotBeNull($"result was not a {typeof(RedirectToRouteResult)}");
            result.RouteName.Should().Be("provider-reservation-created");
        }
    }
}