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
    public class WhenCallingSearchReservations
    {
        [Test, MoqAutoData]
        public async Task Then_Gets_Reservations_From_Api(
            uint providerId,
            ReservationFilter filter,
            [Frozen] Mock<IOptions<ReservationsApiConfiguration>> mockOptions,
            [Frozen] Mock<IApiClient> mockApiClient,
            ReservationService service)
        {
            await service.SearchReservations(providerId, filter);

            mockApiClient.Verify(client => client.Search<SearchReservationResponse>(
                    It.Is<ISearchApiRequest>(request =>
                        request.SearchUrl.StartsWith(mockOptions.Object.Value.Url) &&
                        request.SearchUrl.Contains(providerId.ToString()))),
                Times.Once);
        }

        [Test, MoqAutoData]
        public async Task Then_Returns_Mapped_Reservations(
            uint providerId,
            ReservationFilter filter,
            List<SearchReservationResponse> apiReservations,
            [Frozen] Mock<IApiClient> mockApiClient,
            ReservationService handler)
        {
            mockApiClient
                .Setup(client => client.Search<SearchReservationResponse>(It.IsAny<ISearchApiRequest>()))
                .ReturnsAsync(apiReservations);

            var reservations = await handler.SearchReservations(providerId, filter);

            reservations.Should().BeEquivalentTo(apiReservations);
        }
    }
}