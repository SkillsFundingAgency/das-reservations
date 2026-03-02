using AutoFixture.NUnit3;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Reservations.Domain.Rules.Api;

namespace SFA.DAS.Reservations.Domain.UnitTests.ReservationsApi;

public class WhenBuildingGetAvailableDatesApiRequest
{
    [Test, AutoData]
    public void Then_The_Url_Is_Correctly_Constructed_When_No_CourseId(string baseUrl, int accountLegalEntityId)
    {
        var actual = new GetAvailableDatesApiRequest(baseUrl, accountLegalEntityId);

        actual.GetUrl.Should().Be($"{baseUrl}/rules/available-dates/{accountLegalEntityId}");
    }

    [Test, AutoData]
    public void Then_The_Url_Includes_CourseId_Query_When_CourseId_Provided(string baseUrl, int accountLegalEntityId, string courseId)
    {
        var actual = new GetAvailableDatesApiRequest(baseUrl, accountLegalEntityId, courseId);

        actual.GetUrl.Should().Be($"{baseUrl}/rules/available-dates/{accountLegalEntityId}?courseId={System.Uri.EscapeDataString(courseId)}");
    }
}