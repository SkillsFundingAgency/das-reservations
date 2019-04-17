using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.Reservations.Commands.CacheReservationEmployer;
using SFA.DAS.Reservations.Web.Controllers;
using SFA.DAS.Reservations.Web.Infrastructure;
using SFA.DAS.Reservations.Web.Models;

namespace SFA.DAS.Reservations.Web.UnitTests.Providers
{
    public class WhenConfirmingAnEmployer
    {
        [Test, MoqAutoData]
        public async Task Then_If_Confirmed_The_Choosen_Employer_Is_Stored(
            uint ukPrn,
            [Frozen] Mock<IMediator> mockMediator,
            ProviderReservationsController controller,
            ConfirmEmployerViewModel viewModel)
        {
            viewModel.Confirm = true;

            mockMediator.Setup(m => m.Send(It.IsAny<CacheReservationEmployerCommand>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(Unit.Value);
   
            await controller.ProcessConfirmEmployer(viewModel);

            mockMediator.Verify(m => m.Send(It.Is<CacheReservationEmployerCommand>(c =>
                c.AccountId.Equals(viewModel.AccountId) &&
                c.AccountLegalEntityId.Equals(viewModel.AccountLegalEntityId) &&
                c.AccountLegalEntityPublicHashedId.Equals(viewModel.AccountLegalEntityPublicHashedId) &&
                c.AccountLegalEntityName.Equals(viewModel.AccountLegalEntityName) &&
                c.UkPrn.Equals(viewModel.UkPrn)), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test, MoqAutoData]
        public async Task Then_If_Not_Confirmed_The_Choosen_Employer_Is_Not_Stored(
            uint ukPrn,
            [Frozen] Mock<IMediator> mockMediator,
            ProviderReservationsController controller,
            ConfirmEmployerViewModel viewModel)
        {
            viewModel.Confirm = false;

            mockMediator.Setup(m => m.Send(It.IsAny<CacheReservationEmployerCommand>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(Unit.Value);
   
            await controller.ProcessConfirmEmployer(viewModel);

            mockMediator.Verify(m => m.Send(It.IsAny<CacheReservationEmployerCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test, MoqAutoData]
        public async Task Then_If_Confirmed_User_Is_Redirected_To_Next_Stage(
            uint ukPrn,
            [Frozen] Mock<IMediator> mockMediator,
            ProviderReservationsController controller,
            ConfirmEmployerViewModel viewModel)
        {
            viewModel.Confirm = true;

            mockMediator.Setup(m => m.Send(It.IsAny<CacheReservationEmployerCommand>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(Unit.Value);
   
            var result = await controller.ProcessConfirmEmployer(viewModel);

            var redirectResult = result as RedirectToRouteResult;

            Assert.IsNotNull(redirectResult);
            Assert.AreEqual(RouteNames.ProviderApprenticeshipTraining, redirectResult.RouteName);
            Assert.AreEqual(viewModel.UkPrn,redirectResult.RouteValues["UkPrn"]);
            Assert.AreEqual(viewModel.AccountPublicHashedId,redirectResult.RouteValues["EmployerAccountId"]);
        }

        [Test, MoqAutoData]
        public async Task Then_If_Not_Confirmed_User_Is_Redirected_Back_To_Start_Step(
            uint ukPrn,
            [Frozen] Mock<IMediator> mockMediator,
            ProviderReservationsController controller,
            ConfirmEmployerViewModel viewModel)
        {
            viewModel.Confirm = false;

            mockMediator.Setup(m => m.Send(It.IsAny<CacheReservationEmployerCommand>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(Unit.Value);
   
            var result = await controller.ProcessConfirmEmployer(viewModel);

            var redirectResult = result as RedirectToActionResult;

            Assert.IsNotNull(redirectResult);
            Assert.AreEqual("ProviderReservations",redirectResult.ControllerName);
            Assert.AreEqual("ChooseEmployer",redirectResult.ActionName);
            Assert.AreEqual(viewModel.UkPrn,redirectResult.RouteValues["UkPrn"]);
        }
    }
}
