using System;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.Reservations.Queries.GetReservation;
using SFA.DAS.Reservations.Infrastructure.Configuration.Configuration;
using SFA.DAS.Reservations.Web.Controllers;
using SFA.DAS.Reservations.Web.Infrastructure;
using SFA.DAS.Reservations.Web.Models;

namespace SFA.DAS.Reservations.Web.UnitTests.Reservations
{
    [TestFixture]
    public class WhenCallingGetCompleted
    {
        [Test, MoqAutoData]
        public async Task Then_It_Calls_Mediator_To_Get_Reservation(
            ReservationsRouteModel routeModel,
            [Frozen] Mock<IMediator> mockMediator,
            ReservationsController controller)
        {
            await controller.Completed(routeModel);

            mockMediator.Verify(mediator => mediator.Send(It.Is<GetReservationQuery>(query => query.Id == routeModel.Id), CancellationToken.None));
        }

        [Test, MoqAutoData]
        public async Task Then_It_Returns_The_ViewModel(
            ReservationsRouteModel routeModel,
            GetReservationResult mediatorResult,
            [Frozen] Mock<IMediator> mockMediator,
            [Frozen] Mock<IOptions<ReservationsWebConfiguration>> configuration,
            ReservationsController controller)
        {
            mockMediator
                .Setup(mediator => mediator.Send(It.IsAny<GetReservationQuery>(), CancellationToken.None))
                .ReturnsAsync(mediatorResult);
            
            var result = await controller.Completed(routeModel);

            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            var model = viewResult.Model.Should().BeOfType<CompletedViewModel>().Subject;

            viewResult.ViewName.Should().Be(ViewNames.ProviderCompleted);
        
            model.ReservationId.Should().Be(mediatorResult.ReservationId);
            model.StartDate.Should().Be(mediatorResult.StartDate);
            model.ExpiryDate.Should().Be(mediatorResult.ExpiryDate);
            model.Course.Should().BeEquivalentTo(mediatorResult.Course);
            model.AccountLegalEntityName.Should().BeEquivalentTo(mediatorResult.AccountLegalEntityName);
            model.AccountLegalEntityPublicHashedId.Should().BeEquivalentTo(routeModel.AccountLegalEntityPublicHashedId);
            model.ApprenticeUrl.Should().NotBeEmpty();
            model.DashboardUrl.Should().NotBeEmpty();
        }

        [Test, MoqAutoData]
        public async Task And_No_UkPrn_Then_It_Uses_Employer_View(
            ReservationsRouteModel routeModel,
            GetReservationResult mediatorResult,
            [Frozen] Mock<IMediator> mockMediator,
            [Frozen] Mock<IOptions<ReservationsWebConfiguration>> configuration,
            ReservationsController controller)
        {
            routeModel.UkPrn = null;
            mockMediator
                .Setup(mediator => mediator.Send(It.IsAny<GetReservationQuery>(), CancellationToken.None))
                .ReturnsAsync(mediatorResult);
            
            var result = await controller.Completed(routeModel);

            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.ViewName.Should().Be(ViewNames.EmployerCompleted);
        }

        [Test, MoqAutoData]
        public void Then_If_No_ReservationId_Given_An_Error_Is_Throw(
            ReservationsRouteModel routeModel,
            [Frozen] Mock<IMediator> mockMediator,
            ReservationsController controller)
        {
            routeModel.Id = null;

            Assert.ThrowsAsync<ArgumentException>(() => controller.Completed(routeModel));
        }
    }
}