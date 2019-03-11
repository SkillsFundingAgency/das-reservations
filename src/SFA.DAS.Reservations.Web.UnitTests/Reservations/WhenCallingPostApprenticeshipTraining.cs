using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.Reservations.Commands;
using SFA.DAS.Reservations.Web.Controllers;
using SFA.DAS.Reservations.Web.Models;
using SFA.DAS.Reservations.Web.Services;

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
                    command.AccountId == routeModel.EmployerAccountId &&
                    command.StartDate == formModel.StartDate
                    // todo and course == ...
                    ), It.IsAny<CancellationToken>()));
        }

        [Test, AutoData]//note cannot use autodata to construct controller here due to modelmetadata usage.
        public async Task And_Validation_Error_Then_Returns_Validation_Error_Details(
            ReservationsRouteModel routeModel,
            ApprenticeshipTrainingFormModel formModel,
            Mock<IMediator> mockMediator)
        {
            mockMediator
                .Setup(mediator => mediator.Send(It.IsAny<CacheReservationCommand>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ValidationException(new ValidationResult("Failed", new List<string> { "StartDate|The StartDate field is not valid." }), null, null));
            var controller = new ReservationsController(mockMediator.Object, Mock.Of<IStartDateService>());
            
            var result = await controller.PostApprenticeshipTraining(routeModel, formModel);
            
            Assert.IsNotNull(result);
            var actualViewResult = result as ViewResult;
            Assert.IsNotNull(actualViewResult);
            Assert.IsFalse(actualViewResult.ViewData.ModelState.IsValid);
            Assert.IsTrue(actualViewResult.ViewData.ModelState.ContainsKey("StartDate"));
        }
    }
}