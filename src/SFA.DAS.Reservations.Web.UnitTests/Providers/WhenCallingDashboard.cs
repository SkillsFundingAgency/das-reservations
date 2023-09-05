using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using System.Threading.Tasks;
using FluentAssertions;
using SFA.DAS.GovUK.Auth.Services;
using SFA.DAS.Reservations.Infrastructure.Configuration;
using SFA.DAS.Reservations.Web.Controllers;
using AutoFixture.NUnit3;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Reservations.Web.UnitTests.Providers
{
    [TestFixture]
    public class WhenCallingDashboard
    {
        [Test, MoqAutoData]
        public void Then_It_Calls_Redirect(
            string redirectUrl,
            [Frozen] ReservationsWebConfiguration configuration,
            [NoAutoProperties] HomeController controller)
        {

            configuration.DashboardUrl = redirectUrl;

            var result = controller.Dashboard() as RedirectResult;
            Assert.IsNotNull(result);
            result.Url.Should().Be(redirectUrl);
        }
    }
}
