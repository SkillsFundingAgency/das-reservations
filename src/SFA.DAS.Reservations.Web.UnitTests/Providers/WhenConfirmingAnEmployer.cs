using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.Encoding;
using SFA.DAS.Reservations.Application.Exceptions;
using SFA.DAS.Reservations.Application.Reservations.Commands.CacheReservationEmployer;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Domain.Rules;
using SFA.DAS.Reservations.Domain.Rules.Api;
using SFA.DAS.Reservations.Web.Controllers;
using SFA.DAS.Reservations.Web.Infrastructure;
using SFA.DAS.Reservations.Web.Models;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Reservations.Web.UnitTests.Providers
{
    public class WhenConfirmingAnEmployer
    {
        [Test, MoqAutoData]
        public async Task Then_If_Confirmed_The_Choosen_Employer_Is_Stored(
            uint ukPrn,
            long accountId,
            long accountLegalEntityId,
            [Frozen] Mock<IMediator> mockMediator,
            [Frozen] Mock<IEncodingService> encodingService,
            [NoAutoProperties] ProviderReservationsController controller,
            ConfirmEmployerViewModel viewModel)
        {
            viewModel.Confirm = true;
            encodingService.Setup(x => x.Decode(viewModel.AccountPublicHashedId, EncodingType.PublicAccountId))
                .Returns(accountId);
            encodingService.Setup(x => x.Decode(viewModel.AccountLegalEntityPublicHashedId, EncodingType.PublicAccountLegalEntityId))
                .Returns(accountLegalEntityId);

            mockMediator.Setup(m => m.Send(It.IsAny<CacheReservationEmployerCommand>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(Unit.Value);
   
            await controller.ProcessConfirmEmployer(viewModel);

            mockMediator.Verify(m => m.Send(It.Is<CacheReservationEmployerCommand>(c =>
                c.AccountId.Equals(accountId) &&
                c.AccountLegalEntityId.Equals(accountLegalEntityId) &&
                c.AccountLegalEntityPublicHashedId.Equals(viewModel.AccountLegalEntityPublicHashedId) &&
                c.AccountLegalEntityName.Equals(viewModel.AccountLegalEntityName) &&
                c.AccountName.Equals(viewModel.AccountName) &&
                c.UkPrn.Equals(viewModel.UkPrn)), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test, MoqAutoData]
        public async Task Then_If_Not_Confirmed_The_Choosen_Employer_Is_Not_Stored(
            uint ukPrn,
            [Frozen] Mock<IMediator> mockMediator,
            [NoAutoProperties] ProviderReservationsController controller,
            ConfirmEmployerViewModel viewModel)
        {
            viewModel.Confirm = false;

            mockMediator.Setup(m => m.Send(It.IsAny<CacheReservationEmployerCommand>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(Unit.Value);
   
            await controller.ProcessConfirmEmployer(viewModel);

            mockMediator.Verify(m => m.Send(It.IsAny<CacheReservationEmployerCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test, MoqAutoData]
        public async Task Then_If_Confirmed_And_Funding_Limit_Not_Reached_User_Is_Redirected_To_Next_Stage(
            uint ukPrn,
            [Frozen] Mock<IMediator> mockMediator,
            [Frozen] Mock<IFundingRulesService> mockFundingRulesService,
            [NoAutoProperties] ProviderReservationsController controller,
            ConfirmEmployerViewModel viewModel)
        {
            viewModel.Confirm = true;

            mockMediator.Setup(m => m.Send(It.IsAny<CacheReservationEmployerCommand>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(Unit.Value);

            mockFundingRulesService.Setup(m => m.GetAccountFundingRules(It.IsAny<long>()))
                .ReturnsAsync(new GetAccountFundingRulesApiResponse()
                {
                    GlobalRules = new List<GlobalRule>()
                });
   
            var result = await controller.ProcessConfirmEmployer(viewModel);

            var redirectResult = result as RedirectToRouteResult;

            Assert.IsNotNull(redirectResult);
            Assert.AreEqual(RouteNames.ProviderApprenticeshipTraining, redirectResult.RouteName);
            Assert.AreEqual(viewModel.UkPrn,redirectResult.RouteValues["UkPrn"]);
            Assert.AreEqual(viewModel.AccountPublicHashedId,redirectResult.RouteValues["PublicHashedEmployerAccountId"]);
        }

        [Test, MoqAutoData]
        public async Task Then_If_Confirmed_And_Funding_Limit_Is_Reached_User_Is_Redirected_To_ReservationLimitReached_Page(
            uint ukPrn,
            [Frozen] Mock<IMediator> mockMediator,
            [NoAutoProperties] ProviderReservationsController controller,
            ConfirmEmployerViewModel viewModel)
        {
            viewModel.Confirm = true;

            var validationResult = new Application.Validation.ValidationResult();
            validationResult.AddError(nameof(viewModel.AccountPublicHashedId), "Reservation limit has been reached for this account");

            mockMediator.Setup(m => m.Send(It.IsAny<CacheReservationEmployerCommand>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ReservationLimitReachedException(It.IsAny<long>()));


            var result = await controller.ProcessConfirmEmployer(viewModel);

            var viewResult = result as ViewResult;

            Assert.IsNotNull(viewResult);
            
            Assert.AreEqual("ReservationLimitReached", viewResult.ViewName);
            
        }



        [Test, MoqAutoData]
        public async Task Then_If_Not_Confirmed_User_Is_Redirected_Back_To_Start_Step(
            uint ukPrn,
            [Frozen] Mock<IMediator> mockMediator,
            [NoAutoProperties] ProviderReservationsController controller,
            ConfirmEmployerViewModel viewModel)
        {
            viewModel.Confirm = false;

            mockMediator.Setup(m => m.Send(It.IsAny<CacheReservationEmployerCommand>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(Unit.Value);
   
            var result = await controller.ProcessConfirmEmployer(viewModel);

            var redirectResult = result as RedirectToRouteResult;

            Assert.IsNotNull(redirectResult);
            Assert.AreEqual(RouteNames.ProviderChooseEmployer, redirectResult.RouteName);
            Assert.AreEqual(viewModel.UkPrn,redirectResult.RouteValues["UkPrn"]);
        }
    }
}
