using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.Commitments.Queries.GetCohort;

namespace SFA.DAS.Reservations.Application.UnitTests.Commitments.Queries.GetCohort
{
    public class WhenValidatingGetCohort
    {
        [Test, AutoData]
        public async Task And_No_Id_Then_Invalid(
            GetCohortQueryValidator validator)
        {
            var query = new GetCohortQuery();
            
            var result = await validator.ValidateAsync(query);

            result.IsValid().Should().BeFalse();
            result.ValidationDictionary.Count.Should().Be(1);
            result.ValidationDictionary
                .Should().ContainKey(nameof(GetCohortQuery.CohortId))
                .WhichValue.Should().Be($"{nameof(GetCohortQuery.CohortId)} has not been supplied");
        }

        [Test, AutoData]
        public async Task And_All_Fields_Valid_Then_Valid(
            GetCohortQuery query,
            GetCohortQueryValidator validator)
        {
            var result = await validator.ValidateAsync(query);

            result.IsValid().Should().BeTrue();
            result.ValidationDictionary.Count.Should().Be(0);
        }
    }
}
