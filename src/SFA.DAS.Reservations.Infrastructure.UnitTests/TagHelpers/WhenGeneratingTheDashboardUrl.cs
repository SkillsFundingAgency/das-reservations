using AutoFixture.NUnit3;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Infrastructure.Configuration;
using SFA.DAS.Reservations.Infrastructure.TagHelpers;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Reservations.Infrastructure.UnitTests.TagHelpers
{
    public class WhenGeneratingTheDashboardUrl
    {

        [Test, MoqAutoData]
        public void Then_Uses_EmployerUrl_And_Params_To_Build_Employer_Url(
            string accountId,
            [Frozen] ReservationsWebConfiguration webConfig,
            [Frozen] Mock<IConfiguration> config,
            ExternalUrlHelper urlHelper)
        {
            config.Setup(x => x["AuthType"]).Returns("employer");

            var originalConfigUrl = webConfig.EmployerDashboardUrl;
            webConfig.EmployerDashboardUrl = $"https://{webConfig.EmployerDashboardUrl}";

            var actualUrl = urlHelper.GenerateDashboardUrl(accountId);

            actualUrl.Should().Be(
                $"https://accounts.{originalConfigUrl}/accounts/{accountId}/teams");
        }


        [Test, MoqAutoData]
        public void Then_Uses_ProviderUrl_And_Params_To_Build_Employer_Url(
            [Frozen] ReservationsWebConfiguration webConfig,
            [Frozen] Mock<IConfiguration> config,
            ExternalUrlHelper urlHelper)
        {
            config.Setup(x => x["AuthType"]).Returns("provider");

            webConfig.DashboardUrl = $"https://{webConfig.DashboardUrl}";

            var actualUrl = urlHelper.GenerateDashboardUrl();

            actualUrl.Should().Be(
                $"{webConfig.DashboardUrl}/Account");
        }
    }
}
