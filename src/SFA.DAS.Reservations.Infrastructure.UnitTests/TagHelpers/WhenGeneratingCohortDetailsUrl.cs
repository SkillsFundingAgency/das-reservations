using System;
using AutoFixture.NUnit3;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Infrastructure.Configuration;
using SFA.DAS.Reservations.Infrastructure.TagHelpers;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Reservations.Infrastructure.UnitTests.TagHelpers
{
    public class WhenGeneratingCohortDetailsUrl
    {
        [Test, MoqAutoData]
        public void Then_Uses_No_Folder_When_There_Is_A_Ukprn(
            uint ukprn,
            string cohortRef,
            [Frozen] ReservationsWebConfiguration options,
            [Frozen] Mock<IConfiguration> config,
            ExternalUrlHelper urlHelper)
        {
            config.Setup(x => x["AuthType"]).Returns("provider");
            options.DashboardUrl = $"https://{options.DashboardUrl}";

            var actualUrl = urlHelper.GenerateCohortDetailsUrl(ukprn,"",cohortRef);

            Assert.AreEqual(
                $"{options.DashboardUrl}/{ukprn}/apprentices/{cohortRef}/details",
                actualUrl);
        }

        [Test, MoqAutoData]
        public void Then_Takes_You_Unapproved_Add_Assign_When_There_Is_No_CohortRef(
            string accountId,
            [Frozen] ReservationsWebConfiguration options,
            [Frozen] Mock<IConfiguration> config,
            ExternalUrlHelper urlHelper)
        {
            config.Setup(x => x["AuthType"]).Returns("employer");
            options.EmployerDashboardUrl = $"https://{options.EmployerDashboardUrl}";

            var actualUrl = urlHelper.GenerateCohortDetailsUrl(null, accountId, "");

            Assert.AreEqual(
                $"{options.EmployerDashboardUrl}/commitments/accounts/{accountId}/unapproved/add/assign",
                actualUrl);
        }

        [Test, MoqAutoData]
        public void Then_Uses_Folder_When_There_Is_No_Ukprn(
            string accountId,
            string cohortRef,
            [Frozen] ReservationsWebConfiguration options,
            [Frozen] Mock<IConfiguration> config,
            ExternalUrlHelper urlHelper)
        {
            config.Setup(x => x["AuthType"]).Returns("employer");
            options.EmployerDashboardUrl = $"https://{options.EmployerDashboardUrl}";

            var actualUrl = urlHelper.GenerateCohortDetailsUrl(null, accountId, cohortRef);

            Assert.AreEqual(
                $"{options.EmployerDashboardUrl}/commitments/accounts/{accountId}/apprentices/{cohortRef}/details",
                actualUrl);
        }

        [Test, MoqAutoData]
        public void Then_Uses_Folder_When_There_Is_A_Ukprn_And_Is_Empty_Cohort_Journey(
            string accountId,
            uint ukprn,
            [Frozen] ReservationsWebConfiguration options,
            [Frozen] Mock<IConfiguration> config,
            ExternalUrlHelper urlHelper)
        {
            config.Setup(x => x["AuthType"]).Returns("employer");
            options.EmployerDashboardUrl = $"https://{options.EmployerDashboardUrl}";

            var actualUrl = urlHelper.GenerateCohortDetailsUrl(ukprn, accountId, string.Empty, true);

            Assert.AreEqual(
                $"{options.EmployerDashboardUrl}/commitments/accounts/{accountId}/unapproved/add?providerId={ukprn}",
                actualUrl);
        }
        
        [Test, MoqAutoData]
        public void Then_Uses_Journey_Data(
            string accountId,
            uint ukprn,
            string journeyData,
            [Frozen] ReservationsWebConfiguration options,
            [Frozen] Mock<IConfiguration> config,
            ExternalUrlHelper urlHelper)
        {
            config.Setup(x => x["AuthType"]).Returns("employer");
            options.EmployerDashboardUrl = $"https://{options.EmployerDashboardUrl}";

            var actualUrl = urlHelper.GenerateCohortDetailsUrl(ukprn, accountId, string.Empty, true, journeyData);

            Assert.AreEqual(
                $"{options.EmployerDashboardUrl}/commitments/accounts/{accountId}/unapproved/add?" +
                         $"providerId={ukprn}&journeyData={journeyData}",
                actualUrl);
        }
    }
}
