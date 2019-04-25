using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.Reservations.Queries.GetReservations;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Domain.Reservations.Api;
using SFA.DAS.Reservations.Infrastructure.Api;
using SFA.DAS.Reservations.Infrastructure.Configuration;

namespace SFA.DAS.Reservations.Application.UnitTests.Reservations.Queries.GetReservations
{
    [TestFixture]
    public class WhenGettingReservations
    {
        [Test, MoqAutoData]
        public async Task Then_Gets_Reservations_From_Api(
            long accountId,
            List<GetReservationResponse> apiReservations,
            [Frozen] Mock<IOptions<ReservationsApiConfiguration>> mockOptions,
            [Frozen] Mock<IApiClient> mockApiClient,
            GetReservationsQueryHandler handler)
        {
            var query = new GetReservationsQuery {AccountId = accountId};

            await handler.Handle(query, CancellationToken.None);

            mockApiClient.Verify(client => client.GetAll<GetReservationResponse>(
                It.Is<IGetAllApiRequest>(request => 
                    request.GetAllUrl.StartsWith(mockOptions.Object.Value.Url) && 
                    request.GetAllUrl.Contains(query.AccountId.ToString()))), 
                Times.Once);
        }

        [Test, MoqAutoData]
        public async Task Then_Returns_Mapped_Reservations(
            long accountId,
            List<GetReservationResponse> apiReservations,
            [Frozen] Mock<IOptions<ReservationsApiConfiguration>> mockOptions,
            [Frozen] Mock<IApiClient> mockApiClient,
            GetReservationsQueryHandler handler)
        {
            var query = new GetReservationsQuery { AccountId = accountId };
            mockApiClient
                .Setup(client => client.GetAll<GetReservationResponse>(It.IsAny<IGetAllApiRequest>()))
                .ReturnsAsync(apiReservations);

            var result = await handler.Handle(query, CancellationToken.None);

            result.Reservations.Should().BeEquivalentTo(apiReservations);
        }
    }
}