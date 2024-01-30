using System.Collections.Generic;
using System.Security.Claims;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Infrastructure.Configuration;
using SFA.DAS.Reservations.Web.Controllers;
using SFA.DAS.Reservations.Web.Models;

namespace SFA.DAS.Reservations.Web.UnitTests.Providers
{
    public class WhenVisitingTheAccessDeniedPage
    {
        private Mock<IConfiguration> _configuration;
        private Mock<IOptions<ReservationsWebConfiguration>> _reservationsConfiguration;
        private bool _useDfESignIn;
        private string _dashboardUrl;
        public ErrorController Sut { get; set; }

        [Test]
        [TestCase("test", "https://test-services.signin.education.gov.uk/approvals/select-organisation?action=request-service")]
        [TestCase("pp", "https://test-services.signin.education.gov.uk/approvals/select-organisation?action=request-service")]
        [TestCase("local", "https://test-services.signin.education.gov.uk/approvals/select-organisation?action=request-service")]
        [TestCase("prd", "https://services.signin.education.gov.uk/approvals/select-organisation?action=request-service")]
        public void ThenReturnsTheAccessDeniedModel(string env, string helpLink)
        {
            var fixture = new Fixture();
            _useDfESignIn = fixture.Create<bool>();
            _dashboardUrl = fixture.Create<string>();

            fixture.Customize<ReservationsWebConfiguration>(c => c.With(x => x.UseDfESignIn, _useDfESignIn));
            fixture.Customize<ReservationsWebConfiguration>(c => c.With(x => x.DashboardUrl, _dashboardUrl));

            var mockReservationsConfig = fixture.Create<ReservationsWebConfiguration>();

            _configuration = new Mock<IConfiguration>();

            _reservationsConfiguration = new Mock<IOptions<ReservationsWebConfiguration>>();

            _configuration.Setup(x => x["ResourceEnvironmentName"]).Returns(env);
            _reservationsConfiguration.Setup(ap => ap.Value).Returns(mockReservationsConfig);

            Sut = new ErrorController(_configuration.Object, _reservationsConfiguration.Object);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim> { new Claim("X1", "2") }));
            Sut.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
                { User = user }
            };

            var result = (ViewResult)Sut.AccessDenied();
            result.Should().NotBeNull();

            var actualModel = result?.Model.Should().BeOfType<Error403ViewModel>().Subject;
            actualModel.HelpPageLink.Should().Be(helpLink);
            actualModel.UseDfESignIn.Should().Be(_useDfESignIn);
            actualModel.DashboardUrl.Should().Be(_dashboardUrl);
        }
    }
}
