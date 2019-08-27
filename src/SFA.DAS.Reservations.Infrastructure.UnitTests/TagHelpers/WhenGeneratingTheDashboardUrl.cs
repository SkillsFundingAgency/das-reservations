using System;
using System.Collections.Generic;
using System.Text;
using AutoFixture.NUnit3;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Domain.Interfaces;
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

            Assert.AreEqual(
                $"https://accounts.{originalConfigUrl}/accounts/{accountId}/teams",
                actualUrl);
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

            Assert.AreEqual(
                $"{webConfig.DashboardUrl}/Account",actualUrl);
        }
    }
}
