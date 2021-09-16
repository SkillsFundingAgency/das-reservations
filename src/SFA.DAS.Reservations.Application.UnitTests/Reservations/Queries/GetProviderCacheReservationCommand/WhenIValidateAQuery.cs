using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.Reservations.Queries.GetProviderCacheReservationCommand;

namespace SFA.DAS.Reservations.Application.UnitTests.Reservations.Queries.GetProviderCacheReservationCommand
{
    public class WhenIValidateAQuery
    {
        [Test, AutoData]
        public async Task And_No_Ukprn_Then_Invalid(
            GetProviderCacheReservationCommandQuery query,
            GetProviderCacheReservationCommandQueryValidator validator)
        {
            query.UkPrn = default(uint);
            
            var result = await validator.ValidateAsync(query);

            result.IsValid().Should().BeFalse();
            result.ValidationDictionary.Count.Should().Be(1);
            
            result.ValidationDictionary
                .Should().ContainKey(nameof(GetProviderCacheReservationCommandQuery.UkPrn))
                .WhichValue.Should()
                .Be($"{nameof(GetProviderCacheReservationCommandQuery.UkPrn)} has not been supplied");
        }

        [Test, AutoData]
        public async Task And_No_Legal_Entity_Public_Hashed_Id_Then_Invalid(
            GetProviderCacheReservationCommandQuery query,
            GetProviderCacheReservationCommandQueryValidator validator)
        {
            query.AccountLegalEntityPublicHashedId = null;
            
            var result = await validator.ValidateAsync(query);

            result.IsValid().Should().BeFalse();
            result.ValidationDictionary.Count.Should().Be(1);

            result.ValidationDictionary
                .Should().ContainKey(nameof(GetProviderCacheReservationCommandQuery.AccountLegalEntityPublicHashedId))
                .WhichValue.Should()
                .Be($"{nameof(GetProviderCacheReservationCommandQuery.AccountLegalEntityPublicHashedId)} has not been supplied");
        }

        [Test, AutoData]
        public async Task And_No_Cohort_Reference_Then_Invalid(
            GetProviderCacheReservationCommandQuery query,
            GetProviderCacheReservationCommandQueryValidator validator)
        {
            query.CohortRef = null;
            
            var result = await validator.ValidateAsync(query);

            result.IsValid().Should().BeTrue();
        }

        [Test, AutoData]
        public async Task And_All_Fields_Valid_Then_Valid(
            GetProviderCacheReservationCommandQuery query,
            GetProviderCacheReservationCommandQueryValidator validator)
        {
            var result = await validator.ValidateAsync(query);

            result.IsValid().Should().BeTrue();
            result.ValidationDictionary.Count.Should().Be(0);
        }
    }
}
