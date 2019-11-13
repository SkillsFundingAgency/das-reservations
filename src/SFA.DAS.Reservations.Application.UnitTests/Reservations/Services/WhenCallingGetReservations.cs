using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.Reservations.Services;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Domain.Reservations.Api;
using SFA.DAS.Reservations.Infrastructure.Api;
using SFA.DAS.Reservations.Infrastructure.Configuration;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Reservations.Application.UnitTests.Reservations.Services
{
    [TestFixture]
    public class WhenCallingGetReservations
    {
        [Test, MoqAutoData]
        public async Task Then_Gets_Reservations_From_Api(
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
        public async Task Then_Returns_Mapped_Reservations(
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
    }
}