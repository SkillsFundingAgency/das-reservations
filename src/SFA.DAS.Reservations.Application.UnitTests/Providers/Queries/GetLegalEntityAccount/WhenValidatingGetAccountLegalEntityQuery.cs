using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.Providers.Queries.GetLegalEntityAccount;

namespace SFA.DAS.Reservations.Application.UnitTests.Providers.Queries.GetLegalEntityAccount
{
    public class WhenValidatingGetAccountLegalEntityQuery
    {
        [Test, AutoData]
        public async Task And_No_Id_Then_Invalid(
            GetAccountLegalEntityQueryValidator validator)
        {
            var query = new GetAccountLegalEntityQuery();
            
            var result = await validator.ValidateAsync(query);

            result.IsValid().Should().BeFalse();
            result.ValidationDictionary.Count.Should().Be(1);
            result.ValidationDictionary
                .Should().ContainKey(nameof(GetAccountLegalEntityQuery.AccountLegalEntityPublicHashedId))
                .WhichValue.Should().Be($"{nameof(GetAccountLegalEntityQuery.AccountLegalEntityPublicHashedId)} has not been supplied");
        }

        [Test, AutoData]
        public async Task And_All_Fields_Valid_Then_Valid(
            GetAccountLegalEntityQuery query,
            GetAccountLegalEntityQueryValidator validator)
        {
            var result = await validator.ValidateAsync(query);

            result.IsValid().Should().BeTrue();
            result.ValidationDictionary.Count.Should().Be(0);
        }
    }
}
