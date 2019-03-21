using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.Employers.Queries;

namespace SFA.DAS.Reservations.Application.UnitTests.Employers.Queries
{
    public class WhenValidatingGetTrustedEmployersQuery
    {
        [Test, AutoData]
        public async Task And_No_Id_Then_Invalid(
            GetTrustedEmployerQueryValidator validator)
        {
            var query = new GetTrustedEmployersQuery();
            
            var result = await validator.ValidateAsync(query);

            result.IsValid().Should().BeFalse();
            result.ValidationDictionary.Count.Should().Be(1);
            result.ValidationDictionary
                .Should().ContainKey(nameof(GetTrustedEmployersQuery.UkPrn))
                .WhichValue.Should().Be($"{nameof(GetTrustedEmployersQuery.UkPrn)} has not been supplied");
        }

        [Test, AutoData]
        public async Task And_All_Fields_Valid_Then_Valid(
            GetTrustedEmployersQuery query,
            GetTrustedEmployerQueryValidator validator)
        {
            var result = await validator.ValidateAsync(query);

            result.IsValid().Should().BeTrue();
            result.ValidationDictionary.Count.Should().Be(0);
        }
    }
}
