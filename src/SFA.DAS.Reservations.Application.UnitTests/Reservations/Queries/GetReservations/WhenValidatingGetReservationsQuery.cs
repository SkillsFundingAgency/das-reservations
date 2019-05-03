using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.Reservations.Queries.GetReservations;

namespace SFA.DAS.Reservations.Application.UnitTests.Reservations.Queries.GetReservations
{
    [TestFixture]
    public class WhenValidatingGetReservationsQuery
    {
        [Test, AutoData]
        public async Task And_Account_Id_Is_Default_Value_Then_Invalid(
            GetReservationsQueryValidator validator)
        {
            var query = new GetReservationsQuery();
            
            var result = await validator.ValidateAsync(query);
            
            result.IsValid().Should().BeFalse();
            result.ValidationDictionary.Should().ContainKey(nameof(GetReservationsQuery.AccountId));
        }

        [Test, AutoData]
        public async Task And_Account_Id_Less_Than_0_Then_Invalid(
            long accountId,
            GetReservationsQueryValidator validator)
        {
            var query = new GetReservationsQuery {AccountId = -accountId};

            var result = await validator.ValidateAsync(query);

            result.IsValid().Should().BeFalse();
            result.ValidationDictionary.Should().ContainKey(nameof(GetReservationsQuery.AccountId));
        }

        [Test, AutoData]
        public async Task And_Account_Id_Greater_Than_0_Then_Valid(
            GetReservationsQuery query,
            GetReservationsQueryValidator validator)
        {
            var result = await validator.ValidateAsync(query);

            result.IsValid().Should().BeTrue();
        }
    }
}