using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.Reservations.Queries.GetReservation;

namespace SFA.DAS.Reservations.Application.UnitTests.Reservations.Queries.GetReservation
{
    [TestFixture]
    class WhenValidatingAGetReservationQuery
    {
        [Test, AutoData]
        public async Task And_No_Id_Then_Invalid(
            GetReservationQueryValidator validator)
        {
            var query = new GetReservationQuery();
            
            var result = await validator.ValidateAsync(query);

            result.IsValid().Should().BeFalse();
            result.ValidationDictionary.Count.Should().Be(1);
            result.ValidationDictionary
                .Should().ContainKey(nameof(GetReservationQuery.Id))
                .WhichValue.Should().Be($"{nameof(GetReservationQuery.Id)} has not been supplied");
        }

        [Test, AutoData]
        public async Task And_All_Fields_Valid_Then_Valid(
            GetReservationQuery query,
            GetReservationQueryValidator validator)
        {
            var result = await validator.ValidateAsync(query);

            result.IsValid().Should().BeTrue();
            result.ValidationDictionary.Count.Should().Be(0);
        }
    }
}
