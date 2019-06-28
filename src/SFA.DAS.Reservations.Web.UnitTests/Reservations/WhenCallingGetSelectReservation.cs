using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.Employers.Queries;
using SFA.DAS.Reservations.Application.Reservations.Commands.CreateReservation;
using SFA.DAS.Reservations.Application.Reservations.Commands.CreateReservationLevyEmployer;
using SFA.DAS.Reservations.Application.Reservations.Queries.GetAccountReservationStatus;
using SFA.DAS.Reservations.Application.Reservations.Queries.GetAvailableReservations;
using SFA.DAS.Reservations.Domain.Employers;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Web.Controllers;
using SFA.DAS.Reservations.Web.Infrastructure;
using SFA.DAS.Reservations.Web.Models;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Reservations.Web.UnitTests.Reservations
{
    [TestFixture]
    public class WhenCallingGetSelectReservation
    {
        [Test, MoqAutoData]
        public async Task And_Has_Ukprn_Then_Gets_Employer_From_Trusted_Employers(
            ReservationsRouteModel routeModel,
            SelectReservationViewModel viewModel,
            GetTrustedEmployersResponse employersResponse,
            GetAvailableReservationsResult reservationsResult,
            [Frozen] Mock<IMediator> mockMediator,
            ReservationsController controller)
        {
            var matchedEmployer = employersResponse.Employers.First();
            routeModel.AccountLegalEntityPublicHashedId = matchedEmployer.AccountLegalEntityPublicHashedId;
            mockMediator
                .Setup(mediator => mediator.Send(
                    It.IsAny<GetTrustedEmployersQuery>(), 
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(employersResponse);
            mockMediator
                .Setup(mediator => mediator.Send(
                    It.IsAny<GetAvailableReservationsQuery>(), 
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(reservationsResult);

            await controller.SelectReservation(routeModel, viewModel);

            mockMediator.Verify(mediator => mediator.Send(
                It.Is<GetTrustedEmployersQuery>(query => query.UkPrn == routeModel.UkPrn), 
                It.IsAny<CancellationToken>()));
        }

        [Test, MoqAutoData]
        public async Task And_Has_Ukprn_And_ValidationException_Then_Redirect_To_ProviderError(
            ReservationsRouteModel routeModel,
            SelectReservationViewModel viewModel,
            GetTrustedEmployersResponse employersResponse,
            ValidationException validationException,
            [Frozen] Mock<IMediator> mockMediator,
            ReservationsController controller)
        {
            var matchedEmployer = employersResponse.Employers.First();
            routeModel.AccountLegalEntityPublicHashedId = matchedEmployer.AccountLegalEntityPublicHashedId;
            mockMediator
                .Setup(mediator => mediator.Send(
                    It.IsAny<GetTrustedEmployersQuery>(), 
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(validationException);

            var result = await controller.SelectReservation(routeModel, viewModel) as RedirectToRouteResult;

            result.RouteName.Should().Be(RouteNames.Error500);
        }

        [Test, MoqAutoData]
        public async Task And_Has_Ukprn_And_Exception_Then_Redirect_To_ProviderError(
            ReservationsRouteModel routeModel,
            SelectReservationViewModel viewModel,
            GetTrustedEmployersResponse employersResponse,
            Exception exception,
            [Frozen] Mock<IMediator> mockMediator,
            ReservationsController controller)
        {
            var matchedEmployer = employersResponse.Employers.First();
            routeModel.AccountLegalEntityPublicHashedId = matchedEmployer.AccountLegalEntityPublicHashedId;
            mockMediator
                .Setup(mediator => mediator.Send(
                    It.IsAny<GetTrustedEmployersQuery>(), 
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(exception);

            var result = await controller.SelectReservation(routeModel, viewModel) as RedirectToRouteResult;

            result.RouteName.Should().Be(RouteNames.Error500);
        }

        [Test, MoqAutoData]
        public async Task And_Has_Ukprn_And_Employer_Not_Found_Then_Redirect_To_Error_Page(
            ReservationsRouteModel routeModel,
            SelectReservationViewModel viewModel,
            GetTrustedEmployersResponse employersResponse,
            [Frozen] Mock<IMediator> mockMediator,
            ReservationsController controller)
        {
            mockMediator
                .Setup(mediator => mediator.Send(
                    It.IsAny<GetTrustedEmployersQuery>(), 
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(employersResponse);

            var result = await controller.SelectReservation(routeModel, viewModel) as RedirectToRouteResult;

            result.RouteName.Should().Be(RouteNames.Error500);
        }

        [Test, MoqAutoData]
        public async Task And_Has_Ukprn_Then_Gets_Reservations_For_Employer_Account(
            ReservationsRouteModel routeModel,
            SelectReservationViewModel viewModel,
            GetTrustedEmployersResponse employersResponse,
            GetAvailableReservationsResult reservationsResult,
            [Frozen] Mock<IMediator> mockMediator,
            ReservationsController controller)
        {
            var matchedEmployer = employersResponse.Employers.First();
            routeModel.AccountLegalEntityPublicHashedId = matchedEmployer.AccountLegalEntityPublicHashedId;
            mockMediator
                .Setup(mediator => mediator.Send(
                    It.IsAny<GetTrustedEmployersQuery>(), 
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(employersResponse);
            mockMediator
                .Setup(mediator => mediator.Send(
                    It.IsAny<GetAvailableReservationsQuery>(), 
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(reservationsResult);

            await controller.SelectReservation(routeModel, viewModel);

            mockMediator.Verify(mediator => mediator.Send(
                It.Is<GetAvailableReservationsQuery>(query => query.AccountId == matchedEmployer.AccountId), 
                It.IsAny<CancellationToken>()));
        }

        [Test, MoqAutoData]
        public async Task And_Has_Ukprn_And_Has_Reservations_Then_Shows_ProviderSelect_View(
            ReservationsRouteModel routeModel,
            SelectReservationViewModel viewModel,
            GetTrustedEmployersResponse employersResponse,
            GetAvailableReservationsResult reservationsResult,
            [Frozen] Mock<IMediator> mockMediator,
            ReservationsController controller)
        {
            var matchedEmployer = employersResponse.Employers.First();
            routeModel.AccountLegalEntityPublicHashedId = matchedEmployer.AccountLegalEntityPublicHashedId;
            mockMediator
                .Setup(mediator => mediator.Send(
                    It.IsAny<GetTrustedEmployersQuery>(), 
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(employersResponse);
            mockMediator
                .Setup(mediator => mediator.Send(
                    It.IsAny<GetAvailableReservationsQuery>(), 
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(reservationsResult);

            var result = await controller.SelectReservation(routeModel, viewModel) as ViewResult;

            result.ViewName.Should().Be(ViewNames.ProviderSelect);
            var actualModel = result.Model as SelectReservationViewModel;
            actualModel.Should().NotBeNull();
            actualModel.CohortReference.Should().Be(viewModel.CohortReference);
            actualModel.TransferSenderId.Should().Be(viewModel.TransferSenderId);
            actualModel.AvailableReservations.Should().BeEquivalentTo(
                reservationsResult.Reservations
                    .Select(reservation => new AvailableReservationViewModel(reservation)));
        }
        
        [Test, MoqAutoData]
        public async Task ThenChecksEmployerLevyStatus(
            ReservationsRouteModel routeModel,
            SelectReservationViewModel viewModel,
            [Frozen] GetTrustedEmployersResponse employersResponse,
            GetAvailableReservationsResult reservationsResult,
            GetAccountReservationStatusQuery accountStatusQuery,
            GetAccountReservationStatusResponse accountStatusResponse,
            [Frozen]Mock<IMediator> _mediator,
            ReservationsController controller
            )
        {
            //Arrange
            employersResponse = new GetTrustedEmployersResponse()
            {
                Employers = new List<Employer>()
                {
                    new Employer() {AccountLegalEntityPublicHashedId = routeModel.AccountLegalEntityPublicHashedId}
                }
            };
            accountStatusResponse = new GetAccountReservationStatusResponse()
            {
                CanAutoCreateReservations = true
            };
            _mediator.Setup(x => x.Send(It.IsAny<GetTrustedEmployersQuery>(), CancellationToken.None))
                .ReturnsAsync(employersResponse);
            _mediator.Setup(x => x.Send(accountStatusQuery, CancellationToken.None))
                .ReturnsAsync(accountStatusResponse);


            //Act
            var result = await controller.SelectReservation(routeModel, viewModel);


            //Assert
            _mediator.Verify(x =>
                x.Send(It.IsAny<GetAccountReservationStatusQuery>(),CancellationToken.None),Times.Once());

        }

        [Test, MoqAutoData]
        public async Task AndHasLevyPayingEmployer_ThenCreatesReservationInTheBackground(
            ReservationsRouteModel routeModel,
            SelectReservationViewModel viewModel,
            GetAvailableReservationsResult reservationsResult,
            CreateReservationLevyEmployerResult CreateReservationLevyResult,
            [Frozen]Mock<IMediator> _mediator,
            ReservationsController controller
        )
        {
            //Arrange
            var employersResponse = new GetTrustedEmployersResponse()
            {
                Employers = new List<Employer>()
                {
                    new Employer() {AccountLegalEntityPublicHashedId = routeModel.AccountLegalEntityPublicHashedId}
                }
            };
            var accountStatusResponse = new GetAccountReservationStatusResponse()
            {
                CanAutoCreateReservations = true
            };
            var createReservationLevyResult = new CreateReservationLevyEmployerResult()
            {
                ReservationId = Guid.NewGuid()
            };
            _mediator.Setup(x => x.Send(It.IsAny<GetTrustedEmployersQuery>(), CancellationToken.None))
                .ReturnsAsync(employersResponse);
            _mediator.Setup(x => x.Send(It.IsAny<GetAccountReservationStatusQuery>(), CancellationToken.None))
                .ReturnsAsync(accountStatusResponse);
            _mediator.Setup(x => x.Send(It.IsAny<CreateReservationLevyEmployerCommand>(), CancellationToken.None))
                .ReturnsAsync(createReservationLevyResult);


            //Act
            var result = await controller.SelectReservation(routeModel, viewModel);


            //Assert
            _mediator.Verify(x =>
                x.Send(It.Is<CreateReservationLevyEmployerCommand>(query => query.AccountId == employersResponse.Employers.First().AccountId), CancellationToken.None), Times.Once());

        }

        [Test, MoqAutoData]
        public async Task AndHasLevyPayingEmployerAndReservationCreated_ThenRedirectsToAddAnApprentice(
            ReservationsRouteModel routeModel,
            SelectReservationViewModel viewModel,
            GetAvailableReservationsResult reservationsResult,
            CreateReservationLevyEmployerResult CreateReservationLevyResult,
            [Frozen]Mock<IMediator> _mediator,
            [Frozen]Mock<IExternalUrlHelper> urlHelper,
            string addApprenticeUrl,
            ReservationsController controller
        )
        {
            //Arrange
            var employersResponse = new GetTrustedEmployersResponse()
            {
                Employers = new List<Employer>()
                {
                    new Employer() {AccountLegalEntityPublicHashedId = routeModel.AccountLegalEntityPublicHashedId}
                }
            };
            var accountStatusResponse = new GetAccountReservationStatusResponse()
            {
                CanAutoCreateReservations = true
            };
            var createReservationLevyResult = new CreateReservationLevyEmployerResult()
            {
                ReservationId = Guid.NewGuid()
            };
            _mediator.Setup(x => x.Send(It.IsAny<GetTrustedEmployersQuery>(), CancellationToken.None))
                .ReturnsAsync(employersResponse);
            _mediator.Setup(x => x.Send(It.IsAny<GetAccountReservationStatusQuery>(), CancellationToken.None))
                .ReturnsAsync(accountStatusResponse);
            _mediator.Setup(x => x.Send(It.IsAny<CreateReservationLevyEmployerCommand>(), CancellationToken.None))
                .ReturnsAsync(createReservationLevyResult);

            viewModel.CreateNew = null;
            urlHelper
                .Setup(helper => helper.GenerateAddApprenticeUrl(
                    It.Is<UrlParameters>(parameters =>
                        parameters.Folder == routeModel.UkPrn.ToString() &&
                        parameters.Id == "unapproved" &&
                        parameters.Controller == viewModel.CohortReference &&
                        parameters.Action == "apprentices/add" &&
                        parameters.QueryString == $"?reservationId={createReservationLevyResult.ReservationId}")))
                .Returns(addApprenticeUrl);

            var result = await controller.SelectReservation(routeModel, viewModel) as RedirectResult;
            Assert.IsNotNull(result);
            result?.Url.Should().Be(addApprenticeUrl);
        }



    }
}