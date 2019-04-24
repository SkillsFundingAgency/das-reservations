using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.Reservations.Queries.GetReservations;
using SFA.DAS.Reservations.Domain.Reservations.Api;
using SFA.DAS.Reservations.Infrastructure.Api;

namespace SFA.DAS.Reservations.Application.UnitTests.Reservations.Queries.GetReservations
{
    [TestFixture]
    public class WhenGettingReservations
    {
        [Test, MoqAutoData]
        public async Task Then_Gets_Reservations_From_Api(
            GetReservationsQuery query,
            IEnumerable<GetReservationResponse> apiReservations,
            [Frozen] Mock<ApiClient> mockApiClient,
            GetReservationsQueryHandler handler)
        {
            await handler.Handle(query, CancellationToken.None);

            mockApiClient.Verify(client => client.Get<IEnumerable<GetReservationResponse>>(new ReservationApiRequest()));
        }
    }
}