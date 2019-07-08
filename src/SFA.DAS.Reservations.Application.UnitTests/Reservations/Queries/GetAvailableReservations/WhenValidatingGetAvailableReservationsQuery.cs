using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.Reservations.Queries.GetAvailableReservations;

namespace SFA.DAS.Reservations.Application.UnitTests.Reservations.Queries.GetAvailableReservations
{
    [TestFixture]
    public class WhenValidatingGetAvailableReservationsQuery
    {
        [Test, AutoData]
        public async Task And_Account_Id_Is_Default_Value_Then_Invalid(
            GetAvailableReservationsQueryValidator validator)
        {
            var query = new GetAvailableReservationsQuery();
            
            var result = await validator.ValidateAsync(query);
            
            result.IsValid().Should().BeFalse();
            result.ValidationDictionary.Should().ContainKey(nameof(GetAvailableReservationsQuery.AccountId));
        }

        [Test, AutoData]
        public async Task And_Account_Id_Less_Than_0_Then_Invalid(
            long accountId,
            GetAvailableReservationsQueryValidator validator)
        {
            var query = new GetAvailableReservationsQuery {AccountId = -accountId};

            var result = await validator.ValidateAsync(query);

            result.IsValid().Should().BeFalse();
            result.ValidationDictionary.Should().ContainKey(nameof(GetAvailableReservationsQuery.AccountId));
        }

        [Test, AutoData]
        public async Task And_Account_Id_Greater_Than_0_Then_Valid(
            GetAvailableReservationsQuery query,
            GetAvailableReservationsQueryValidator validator)
        {
            var result = await validator.ValidateAsync(query);

            result.IsValid().Should().BeTrue();
        }
    }
}