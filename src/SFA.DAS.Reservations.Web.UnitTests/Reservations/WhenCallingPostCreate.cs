using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
using SFA.DAS.Reservations.Web.Services;

namespace SFA.DAS.Reservations.Web.UnitTests.Reservations
{
    [TestFixture]
    public class WhenCallingPostCreate
    {
        private IFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization { ConfigureMembers = true });
        }

        [Test, AutoData]
        public async Task Then_Sends_Command_With_Correct_Values_Set(string employerAccountId, CreateReservationResult createReservationResult)
        {
            var mockMediator = _fixture.Freeze<Mock<IMediator>>();
            mockMediator
                .Setup(mediator => mediator.Send(It.IsAny<CreateReservationCommand>(), CancellationToken.None))
                .ReturnsAsync(createReservationResult);
            var controller = _fixture.Create<ReservationsController>();
            var expectedStartDate = "2018-10";

            await controller.Create(employerAccountId,null, expectedStartDate);

            mockMediator.Verify(mediator => 
                mediator.Send(It.Is<CreateReservationCommand>(command => 
                    command.AccountId == employerAccountId && 
                    command.StartDate == expectedStartDate
                        ), It.IsAny<CancellationToken>()));
        }

        [Test, AutoData]
        public async Task Then_Redirects_To_The_Confirmation_Employer_View_When_No_UkPrn(string employerAccountId, CreateReservationResult createReservationResult)
        {
            var mockMediator = _fixture.Freeze<Mock<IMediator>>();
            mockMediator
                .Setup(mediator => mediator.Send(It.IsAny<CreateReservationCommand>(), CancellationToken.None))
                .ReturnsAsync(createReservationResult);
            var controller = _fixture.Create<ReservationsController>();

            var result = await controller.Create(employerAccountId,null, "2018-10") as RedirectToRouteResult;

            result.Should().NotBeNull($"result was not a {typeof(RedirectToRouteResult)}");
            result.RouteName.Should().Be("employer-reservation-created");
            result.RouteValues.Should().ContainKey("id").WhichValue.Should().NotBe(Guid.Empty);
        }

        [Test, AutoData]
        public async Task Then_Redirects_To_The_Confirmation_Provider_View_When_Has_UkPrn(string employerAccountId, int ukPrn, CreateReservationResult createReservationResult)
        {
            var mockMediator = _fixture.Freeze<Mock<IMediator>>();
            mockMediator
                .Setup(mediator => mediator.Send(It.IsAny<CreateReservationCommand>(), CancellationToken.None))
                .ReturnsAsync(createReservationResult);
            var controller = _fixture.Create<ReservationsController>();

            var result = await controller.Create(employerAccountId, ukPrn, "2018-10") as RedirectToRouteResult;

            result.Should().NotBeNull($"result was not a {typeof(RedirectToRouteResult)}");
            result.RouteName.Should().Be("provider-reservation-created");
            result.RouteValues.Should().ContainKey("id").WhichValue.Should().NotBe(Guid.Empty);
        }

        [Test, AutoData]
        public async Task Then_Redisplays_The_View_If_There_Is_A_Validation_Error_From_The_Command(string employerAccountId)
        {
            var mediator = new Mock<IMediator>();
            mediator.Setup(x => x.Send(It.IsAny<CreateReservationCommand>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ValidationException(new ValidationResult("Failed", new List<string> { "TrainingStartDate|The TrainingStartDate field is not valid." }), null, null));
            var controller = new ReservationsController(mediator.Object, Mock.Of<IStartDateService>());

            var actual = await controller.Create("123r", null, "201");

            Assert.IsNotNull(actual);
            var actualViewResult = actual as ViewResult;
            Assert.IsNotNull(actualViewResult);
            Assert.IsFalse(actualViewResult.ViewData.ModelState.IsValid);
            Assert.IsTrue(actualViewResult.ViewData.ModelState.ContainsKey("TrainingStartDate"));
        }
    }
}