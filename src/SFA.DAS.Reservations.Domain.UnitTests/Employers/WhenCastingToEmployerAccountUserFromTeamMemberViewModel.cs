using AutoFixture.NUnit3;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EAS.Account.Api.Types;
using SFA.DAS.Reservations.Domain.Employers;

namespace SFA.DAS.Reservations.Domain.UnitTests.Employers
{
    public class WhenCastingToEmployerAccountUserFromTeamMemberViewModel
    {
        [Test, AutoData]
        public void Then(TeamMemberViewModel source)
        {
            EmployerAccountUser result = source;

            result.Should().BeEquivalentTo(source, o => o.Excluding(x => x.Status));
            result.Status.Should().Be((int) source.Status);
        }
    }
}