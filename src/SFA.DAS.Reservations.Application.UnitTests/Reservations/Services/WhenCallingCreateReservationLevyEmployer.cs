using System;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.Reservations.Services;
using SFA.DAS.Reservations.Domain.Reservations.Api;
using SFA.DAS.Reservations.Infrastructure.Api;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Reservations.Application.UnitTests.Reservations.Services
{
    [TestFixture]
    public class WhenCallingCreateReservationLevyEmployer
    {
        [Test, MoqAutoData]
        public async Task Then_Calls_Api_Client_With_Correct_Request(
            Guid id,
            long accountId,
            long accountLegalEntityId,
            long transferSenderAccountId,
            Guid userId,
            [Frozen] Mock<IApiClient> mockApiClient,
            ReservationService service)
        {
            //Arrange
            mockApiClient.Setup(x => x.Create<CreateReservationResponse>(It.IsAny<ReservationApiRequest>()))
                .ReturnsAsync(new CreateReservationResponse() { Id = id });
            //Act
            await service.CreateReservationLevyEmployer(id, accountId, accountLegalEntityId, transferSenderAccountId, userId);

            //Assert
            mockApiClient.Verify(x => x.Create<CreateReservationResponse>(It.Is<ReservationApiRequest>(
                request => request.Id == id &&
                           request.AccountId == accountId &&
                           request.AccountLegalEntityId == accountLegalEntityId && 
                           request.TransferSenderAccountId == transferSenderAccountId && 
                           request.UserId == userId && 
                           request.IsLevyAccount)));
        }

        [Test, MoqAutoData]
        public async Task Then_Returns_Correct_Response(
            Guid id,
            long accountId,
            long accountLegalEntityId,
            Guid userId,
            [Frozen] Mock<IApiClient> mockApiClient,
            ReservationService service)
        {
            //Arrange
            mockApiClient.Setup(x => x.Create<CreateReservationResponse>(It.IsAny<ReservationApiRequest>()))
                .ReturnsAsync(new CreateReservationResponse() { Id = id });
            //Act
            var result = await service.CreateReservationLevyEmployer(id, accountId, accountLegalEntityId, null, userId);

            //Assert
            result.Id.Should().Be(id);
        }
    }
}