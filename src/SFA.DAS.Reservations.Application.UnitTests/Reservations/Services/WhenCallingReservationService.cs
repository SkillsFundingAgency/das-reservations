using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.Reservations.Services;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Domain.Reservations;
using SFA.DAS.Reservations.Domain.Reservations.Api;
using SFA.DAS.Reservations.Infrastructure.Api;
using SFA.DAS.Reservations.Infrastructure.Configuration;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Reservations.Application.UnitTests.Reservations.Services
{
    [TestFixture]
    public class WhenCallingReservationService
    {
        [Test, MoqAutoData]
        public async Task AndGettingReservations_ThenGetsReservationsFromApi(
            long accountId,
            [Frozen] Mock<IOptions<ReservationsApiConfiguration>> mockOptions,
            [Frozen] Mock<IApiClient> mockApiClient,
            ReservationService service)
        {
            await service.GetReservations(accountId);

            mockApiClient.Verify(client => client.GetAll<GetReservationResponse>(
                    It.Is<IGetAllApiRequest>(request =>
                        request.GetAllUrl.StartsWith(mockOptions.Object.Value.Url) &&
                        request.GetAllUrl.Contains(accountId.ToString()))),
                Times.Once);
        }

        [Test, MoqAutoData]
        public async Task AndGettingReservations_ThenReturnsMappedReservations(
            long accountId,
            List<GetReservationResponse> apiReservations,
            [Frozen] Mock<IApiClient> mockApiClient,
            ReservationService handler)
        {
            mockApiClient
                .Setup(client => client.GetAll<GetReservationResponse>(It.IsAny<IGetAllApiRequest>()))
                .ReturnsAsync(apiReservations);

            var reservations = await handler.GetReservations(accountId);

            reservations.Should().BeEquivalentTo(apiReservations);
        }

        [Test, MoqAutoData]
        public async Task AndCreatingALevyReservation_ThenCallsApiClientWithCorrectRequest(
            Guid id,
            long accountId,
            uint providerId,
            DateTime startDate,
            long accountLegalEntityId,
            string accountLegalEntityName,
            [Frozen] Mock<IApiClient> mockApiClient,
            ReservationService service)
        {
            //Arrange
            mockApiClient.Setup(x => x.Create<CreateReservationResponse>(It.IsAny<ReservationApiRequest>()))
                .ReturnsAsync(new CreateReservationResponse() { Id = id });
            //Act
            await service.CreateReservationLevyEmployer(id, accountId, accountLegalEntityId);

            //Assert
            mockApiClient.Verify(x => x.Create<CreateReservationResponse>(It.Is<ReservationApiRequest>(
                request => request.Id == id &&
                           request.AccountId == accountId &&
                           request.AccountLegalEntityId == accountLegalEntityId)));
        }


        [Test, MoqAutoData]
        public async Task AndCreatingALevyReservation_ThenReturnsCorrectResponse(
            Guid id,
            long accountId,
            long accountLegalEntityId,
            [Frozen] Mock<IApiClient> mockApiClient,
            ReservationService service)
        {
            //Arrange
            mockApiClient.Setup(x => x.Create<CreateReservationResponse>(It.IsAny<ReservationApiRequest>()))
                .ReturnsAsync(new CreateReservationResponse() { Id = id });
            //Act
            var result = await service.CreateReservationLevyEmployer(id, accountId, accountLegalEntityId);

            //Assert
            Assert.AreEqual(id, result.Id);
        }
    }
}