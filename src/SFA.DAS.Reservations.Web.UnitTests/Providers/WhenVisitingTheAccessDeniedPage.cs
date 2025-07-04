using AutoFixture;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Infrastructure.Configuration;
using SFA.DAS.Reservations.Web.Controllers;
using SFA.DAS.Reservations.Web.Models;

namespace SFA.DAS.Reservations.Web.UnitTests.Providers;

public class WhenVisitingTheAccessDeniedPage
{
    private Mock<IConfiguration> _configuration;
    private Mock<IOptions<ReservationsWebConfiguration>> _reservationsConfiguration;
    private string _dashboardUrl;
    private ErrorController Sut { get; set; }

    [Test]
    [TestCase("test", "https://test-services.signin.education.gov.uk/approvals/select-organisation?action=request-service")]
    [TestCase("pp", "https://test-services.signin.education.gov.uk/approvals/select-organisation?action=request-service")]
    [TestCase("local", "https://test-services.signin.education.gov.uk/approvals/select-organisation?action=request-service")]
    [TestCase("prd", "https://services.signin.education.gov.uk/approvals/select-organisation?action=request-service")]
    public void ThenReturnsTheAccessDeniedModel(string env, string helpLink)
    {
        var fixture = new Fixture();
        _dashboardUrl = fixture.Create<string>();

        var mockReservationsConfig = new ReservationsWebConfiguration
        {
            DashboardUrl = _dashboardUrl
        };

        _configuration = new Mock<IConfiguration>();
        _reservationsConfiguration = fixture.Freeze<Mock<IOptions<ReservationsWebConfiguration>>>();

        _configuration.Setup(x => x["ResourceEnvironmentName"]).Returns(env);
        _reservationsConfiguration.Setup(ap => ap.Value).Returns(mockReservationsConfig);

        Sut = new ErrorController(_configuration.Object, _reservationsConfiguration.Object);

        var result = (ViewResult)Sut.AccessDenied();

        Assert.That(result, Is.Not.Null);
        var actualModel = result?.Model as Error403ViewModel;
        Assert.That(actualModel?.HelpPageLink, Is.EqualTo(helpLink));
        Assert.AreEqual(actualModel?.DashboardUrl, _dashboardUrl);
    }
}