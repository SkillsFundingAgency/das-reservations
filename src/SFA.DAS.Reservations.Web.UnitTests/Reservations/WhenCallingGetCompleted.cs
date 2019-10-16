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
using SFA.DAS.Reservations.Domain.Rules;
using SFA.DAS.Reservations.Infrastructure.Configuration;
using SFA.DAS.Reservations.Web.Controllers;
using SFA.DAS.Reservations.Web.Extensions;
using SFA.DAS.Reservations.Web.Infrastructure;
using SFA.DAS.Reservations.Web.Models;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Reservations.Web.UnitTests.Reservations
{
    [TestFixture]
    public class WhenCallingGetCompleted
    {
        [Test, MoqAutoData]
        public async Task Then_It_Calls_Mediator_To_Get_Reservation(
            ReservationsRouteModel routeModel,
            GetReservationResult mediatorResult,
            [Frozen] Mock<IMediator> mockMediator,
            ReservationsController controller)
        {
            mockMediator
                .Setup(mediator => mediator.Send(It.IsAny<GetReservationQuery>(), CancellationToken.None))
                .ReturnsAsync(mediatorResult);

            await controller.Completed(routeModel);

            mockMediator.Verify(mediator => mediator.Send(It.Is<GetReservationQuery>(query => query.Id == routeModel.Id), CancellationToken.None));
        }

        [Test, MoqAutoData]
        public async Task Then_It_Returns_The_ViewModel(
            ReservationsRouteModel routeModel,
            GetReservationResult mediatorResult,
            [Frozen] Mock<IMediator> mockMediator,
            [Frozen] Mock<IOptions<ReservationsWebConfiguration>> mockConfig,
            ReservationsController controller)
        {
            mockMediator
                .Setup(mediator => mediator.Send(It.IsAny<GetReservationQuery>(), CancellationToken.None))
                .ReturnsAsync(mediatorResult);
            
            var result = await controller.Completed(routeModel);

            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            var model = viewResult.Model.Should().BeOfType<CompletedViewModel>().Subject;

            viewResult.ViewName.Should().Be(ViewNames.ProviderCompleted);

            model.AccountLegalEntityName.Should().Be(mediatorResult.AccountLegalEntityName);
            model.TrainingDateDescription.Should().Be(new TrainingDateModel
            {
                StartDate = mediatorResult.StartDate,
                EndDate = mediatorResult.ExpiryDate
            }.GetGDSDateString());
            model.CourseDescription.Should().Be(mediatorResult.Course.CourseDescription);
            model.StartDate.Should().Be(mediatorResult.StartDate);
            model.CourseId.Should().Be(mediatorResult.Course.Id);
            model.UkPrn.Should().Be(mediatorResult.UkPrn);
            model.CohortRef.Should().Be(routeModel.CohortReference);
        }

        [Test, MoqAutoData]
        public async Task And_No_UkPrn_Then_It_Uses_Employer_View_And_Uses_Provider_Id_If_Not_Null(
            ReservationsRouteModel routeModel,
            GetReservationResult mediatorResult,
            [Frozen] Mock<IMediator> mockMediator,
            [Frozen] Mock<IOptions<ReservationsWebConfiguration>> configuration,
            ReservationsController controller)
        {
            routeModel.UkPrn = null;
            mediatorResult.UkPrn = null;
            mockMediator
                .Setup(mediator => mediator.Send(It.IsAny<GetReservationQuery>(), CancellationToken.None))
                .ReturnsAsync(mediatorResult);
            
            var result = await controller.Completed(routeModel);

            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.ViewName.Should().Be(ViewNames.EmployerCompleted);
            var model = viewResult.Model.Should().BeOfType<CompletedViewModel>().Subject;
            model.UkPrn.Should().Be(routeModel.ProviderId);
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