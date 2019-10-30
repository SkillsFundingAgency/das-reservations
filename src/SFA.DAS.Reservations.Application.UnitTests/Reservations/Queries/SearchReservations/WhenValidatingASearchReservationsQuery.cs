using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.Reservations.Queries.SearchReservations;

namespace SFA.DAS.Reservations.Application.UnitTests.Reservations.Queries.SearchReservations
{
    public class WhenValidatingASearchReservationsQuery
    {
        [Test, AutoData]
        public async Task Then_Is_Valid(
            SearchReservationsQuery query,
            SearchReservationsQueryValidator validator)
        {
            var result = await validator.ValidateAsync(query);

            result.IsValid().Should().BeTrue();
        }
    }
}