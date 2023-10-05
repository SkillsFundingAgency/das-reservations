using AutoFixture.NUnit3;
using NUnit.Framework;
using SFA.DAS.Reservations.Domain.Providers.Api;
using FluentAssertions;

namespace SFA.DAS.Reservations.Domain.UnitTests.ReservationsApi
{
    public class WhenBuildingGetProviderAccountRequest
    {
        [Test, AutoData]
        public void Then_The_Url_Is_Correctly_Constructed(string baseUrl, int ukprn)
        {
            var actual = new GetProviderStatusDetails(baseUrl, ukprn);

            actual.GetUrl.Should().Be($"provideraccounts/{ukprn}");
        }
    }
}
