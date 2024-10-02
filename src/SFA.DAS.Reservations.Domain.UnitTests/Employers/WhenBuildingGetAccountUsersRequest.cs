using AutoFixture.NUnit3;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Reservations.Domain.Employers.Api;

namespace SFA.DAS.Reservations.Domain.UnitTests.Employers;

public class WhenBuildingGetAccountUsersRequest
{
    [Test, AutoData]
    public void Then_The_Url_Is_Constructed_Correctly(long accountId, string baseUrl)
    {
        var actual = new GetAccountUsersRequest(baseUrl, accountId);

        actual.GetUrl.Should().Be($"{baseUrl}/accounts/{accountId}/users");
    }
}