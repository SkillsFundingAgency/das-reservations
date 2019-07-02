using System;
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
using SFA.DAS.Encoding;
using SFA.DAS.Reservations.Application.Employers.Queries;
using SFA.DAS.Reservations.Application.Employers.Queries.GetLegalEntities;
using SFA.DAS.Reservations.Application.Exceptions;
using SFA.DAS.Reservations.Application.Reservations.Commands.CacheReservationEmployer;
using SFA.DAS.Reservations.Application.Reservations.Queries.GetAvailableReservations;
using SFA.DAS.Reservations.Domain.Reservations;
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
        public async Task And_Has_AccountId_And_Employer_Not_Found_Then_Redirect_To_Error_Page(
            ReservationsRouteModel routeModel,
            SelectReservationViewModel viewModel,
            GetLegalEntitiesResponse employersResponse,
            [Frozen] Mock<IMediator> mockMediator,
            ReservationsController controller)
        {
            routeModel.UkPrn = null;
            mockMediator
                .Setup(mediator => mediator.Send(
                    It.IsAny<GetLegalEntitiesQuery>(),
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
        public async Task Then_If_There_Are_No_Available_Reservations_For_A_Provider_Then_The_Cache_Is_Created_And_Redirected_To_Choose_Course(
            ReservationsRouteModel routeModel,
            SelectReservationViewModel viewModel,
            GetTrustedEmployersResponse employersResponse,
            GetAvailableReservationsResult reservationsResult,
            [Frozen] Mock<IMediator> mockMediator,
            long expectedAccountId,
            long expectedAccountLegalEntityId,
            ReservationsController controller)
        {
            //Arrange
            routeModel.Id = Guid.Empty;
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
                .ReturnsAsync(new GetAvailableReservationsResult {Reservations = new List<Reservation>()});

            //Act
            var result = await controller.SelectReservation(routeModel, viewModel) as RedirectToRouteResult;

            //Assert
            mockMediator.Verify(x=>
                x.Send(It.Is<CacheReservationEmployerCommand>(
                        c=>
                           c.CohortRef.Equals(viewModel.CohortReference) &&
                           c.AccountId.Equals(matchedEmployer.AccountId) &&
                           c.AccountLegalEntityName.Equals(matchedEmployer.AccountLegalEntityName) &&
                           c.AccountLegalEntityPublicHashedId.Equals(matchedEmployer.AccountLegalEntityPublicHashedId) &&
                           c.AccountName.Equals(matchedEmployer.AccountName) &&
                           c.UkPrn.Equals(routeModel.UkPrn) &&
                           !c.Id.Equals(Guid.Empty) &&
                           c.AccountLegalEntityId.Equals(matchedEmployer.AccountLegalEntityId))
                    , It.IsAny<CancellationToken>()), Times.Once);
            Assert.IsNotNull(result);
            Assert.AreEqual(RouteNames.ProviderApprenticeshipTraining,result.RouteName);
            Assert.AreNotEqual(Guid.Empty, routeModel.Id);
        }

        [Test, MoqAutoData]
        public async Task Then_If_There_Are_No_Available_Reservations_For_A_Employer_Then_The_Cache_Is_Created_And_Redirected_To_Choose_Course(
            ReservationsRouteModel routeModel,
            SelectReservationViewModel viewModel,
            GetLegalEntitiesResponse employersResponse,
            GetAvailableReservationsResult reservationsResult,
            [Frozen] Mock<IMediator> mockMediator,
            long expectedAccountId,
            long expectedAccountLegalEntityId,
            [Frozen] Mock<IEncodingService> encodingService,
            ReservationsController controller)
        {
            //Arrange
            routeModel.Id = Guid.Empty;
            routeModel.UkPrn = null;
            encodingService.Setup(x => x.Decode(routeModel.EmployerAccountId, EncodingType.AccountId)).Returns(expectedAccountId);
            var matchedEmployer = employersResponse.AccountLegalEntities.First();
            routeModel.AccountLegalEntityPublicHashedId = matchedEmployer.AccountLegalEntityPublicHashedId;
            mockMediator
                .Setup(mediator => mediator.Send(
                    It.Is<GetLegalEntitiesQuery>(c=>c.AccountId.Equals(expectedAccountId)),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(employersResponse);
            mockMediator
                .Setup(mediator => mediator.Send(
                    It.Is<GetAvailableReservationsQuery>(c=>c.AccountId.Equals(expectedAccountId)),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetAvailableReservationsResult { Reservations = new List<Reservation>() });

            //Act
            var result = await controller.SelectReservation(routeModel, viewModel) as RedirectToRouteResult;

            //Assert
            mockMediator.Verify(x =>
                x.Send(It.Is<CacheReservationEmployerCommand>(
                        c =>
                            c.CohortRef.Equals(viewModel.CohortReference) &&
                            c.AccountId.Equals(expectedAccountId) &&
                            c.AccountLegalEntityName.Equals(matchedEmployer.AccountLegalEntityName) &&
                            c.AccountLegalEntityPublicHashedId.Equals(matchedEmployer.AccountLegalEntityPublicHashedId) &&
                            !c.Id.Equals(Guid.Empty) &&
                            c.AccountLegalEntityId.Equals(matchedEmployer.AccountLegalEntityId))
                    , It.IsAny<CancellationToken>()), Times.Once);
            Assert.IsNotNull(result);
            Assert.AreEqual(RouteNames.EmployerApprenticeshipTraining, result.RouteName);
            Assert.AreNotEqual(Guid.Empty, routeModel.Id);
        }

        [Test, MoqAutoData]
        public async Task Then_If_The_Reservation_Limit_Has_Been_Reached_The_Reservation_Limit_View_Is_Returned(
                ReservationsRouteModel routeModel,
                SelectReservationViewModel viewModel,
                GetLegalEntitiesResponse employersResponse,
                GetAvailableReservationsResult reservationsResult,
                [Frozen] Mock<IMediator> mockMediator,
                long expectedAccountId,
                long expectedAccountLegalEntityId,
                [Frozen] Mock<IEncodingService> encodingService,
                ReservationsController controller)
        {
            //Arrange
            routeModel.Id = Guid.Empty;
            routeModel.UkPrn = null;
            encodingService.Setup(x => x.Decode(routeModel.EmployerAccountId, EncodingType.AccountId)).Returns(expectedAccountId);
            var matchedEmployer = employersResponse.AccountLegalEntities.First();
            routeModel.AccountLegalEntityPublicHashedId = matchedEmployer.AccountLegalEntityPublicHashedId;
            mockMediator
                .Setup(mediator => mediator.Send(
                    It.Is<GetLegalEntitiesQuery>(c => c.AccountId.Equals(expectedAccountId)),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(employersResponse);
            mockMediator
                .Setup(mediator => mediator.Send(
                    It.Is<GetAvailableReservationsQuery>(c => c.AccountId.Equals(expectedAccountId)),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetAvailableReservationsResult {Reservations = new List<Reservation>()});
            mockMediator.Setup(x => x.Send(It.IsAny<CacheReservationEmployerCommand>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ReservationLimitReachedException(expectedAccountId));

            //Act
            var result = await controller.SelectReservation(routeModel, viewModel) as ViewResult;

            //Assert
            result.ViewName.Should().Be("ReservationLimitReached");
        }
    }
}