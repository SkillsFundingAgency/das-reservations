using AutoFixture.NUnit3;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Reservations.Domain.Employers;
using SFA.DAS.Reservations.Domain.Employers.Api;

namespace SFA.DAS.Reservations.Domain.UnitTests.Employers;

public class WhenCastingToEmployerAccountUserFromAccountUsersResponseItem
{
    [Test, AutoData]
    public void Then(AccountUsersResponseItem source)
    {
        var result = (EmployerAccountUser)source;

        result.Should().BeEquivalentTo(source);
    }
}