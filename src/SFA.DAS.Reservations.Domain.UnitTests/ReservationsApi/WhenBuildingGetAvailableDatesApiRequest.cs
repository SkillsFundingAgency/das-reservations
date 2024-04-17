using AutoFixture.NUnit3;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Reservations.Domain.Rules.Api;

namespace SFA.DAS.Reservations.Domain.UnitTests.ReservationsApi
{
    public class WhenBuildingGetAvailableDatesApiRequest
    {
        [Test, AutoData]
        public void Then_The_Url_Is_Correctly_Constructed(string baseUrl, int accountLegalEntityId)
        {
            var actual = new GetAvailableDatesApiRequest(baseUrl, accountLegalEntityId);

            actual.GetUrl.Should().Be($"{baseUrl}/rules/available-dates/{accountLegalEntityId}");
        }
    }
}
