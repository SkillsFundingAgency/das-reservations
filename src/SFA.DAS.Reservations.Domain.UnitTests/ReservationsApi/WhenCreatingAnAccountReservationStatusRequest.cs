using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Reservations.Domain.Reservations.Api;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Reservations.Domain.UnitTests.ReservationsApi
{
    [TestFixture]
    public class WhenCreatingAnAccountReservationStatusRequest
    {
        [Test,MoqAutoData]
        public void ThenConstructsTheCorrectUrl(string baseUrl, long accountId)
        {
            var expectedUrl = $"{baseUrl}api/accounts/{accountId}/status";
            var request = new AccountReservationStatusRequest(baseUrl,accountId);
            request.GetUrl.Should().Be(expectedUrl);
        }
    }
}
