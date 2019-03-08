using System.Threading;
using System.Threading.Tasks;
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
    public class WhenCallingPostApprenticeshipTraining
    {
        [Test, MoqAutoData]
        public async Task And_Has_Ukprn_Then_Redirects_To_Provider_Route(
            ReservationsRouteModel routeModel,
            ApprenticeshipTrainingFormModel formModel,
            ReservationsController controller)
        {
            var result = await controller.PostApprenticeshipTraining(routeModel, formModel) as RedirectToRouteResult;
            
            result.Should().NotBeNull($"result was not a {typeof(RedirectToRouteResult)}");
            result.RouteName.Should().Be("provider-review");
        }

        [Test, MoqAutoData]
        public async Task And_No_Ukprn_Then_Redirects_To_Employer_Route(
            ReservationsRouteModel routeModel,
            ApprenticeshipTrainingFormModel formModel,
            ReservationsController controller)
        {
            routeModel.Ukprn = null;
            var result = await controller.PostApprenticeshipTraining(routeModel, formModel) as RedirectToRouteResult;
            
            result.Should().NotBeNull($"result was not a {typeof(RedirectToRouteResult)}");
            result.RouteName.Should().Be("employer-review");
        }

        [Test, MoqAutoData]
        public async Task Then_Caches_Draft_Reservation(
            ReservationsRouteModel routeModel,
            ApprenticeshipTrainingFormModel formModel,
            [Frozen] Mock<IMediator> mockMediator, 
            ReservationsController controller)
        {
            await controller.PostApprenticeshipTraining(routeModel, formModel);

            mockMediator.Verify(mediator => 
                mediator.Send(It.Is<CacheReservationCommand>(command => 
                    command.StartDate == formModel.StartDate
                    // todo and course == ...
                    ), It.IsAny<CancellationToken>()));
        }
    }
}