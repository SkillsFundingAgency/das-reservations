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
            options.ApprenticeUrl = $"https://{options.ApprenticeUrl}";

            var actualUrl = urlHelper.GenerateCohortDetailsUrl(ukprn,"",cohortRef);

            actualUrl.Should().Be(
                $"{options.ApprenticeUrl}/{ukprn}/unapproved/{cohortRef}/details");
        }

        [Test, MoqAutoData]
        public void Then_Takes_You_Unapproved_Add_Assign_When_There_Is_No_CohortRef(
            string accountId,
            [Frozen] ReservationsWebConfiguration options,
            [Frozen] Mock<IConfiguration> config,
            ExternalUrlHelper urlHelper)
        {
            config.Setup(x => x["AuthType"]).Returns("employer");
            options.EmployerApprenticeUrl = $"https://{options.EmployerApprenticeUrl}";

            var actualUrl = urlHelper.GenerateCohortDetailsUrl(null, accountId, "", true);

            actualUrl.Should().Be(
                $"{options.EmployerApprenticeUrl}/{accountId}/unapproved/add/assign");
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
            options.EmployerApprenticeUrl = $"https://{options.EmployerApprenticeUrl}";

            var actualUrl = urlHelper.GenerateCohortDetailsUrl(null, accountId, cohortRef);

            actualUrl.Should().Be(
                $"{options.EmployerApprenticeUrl}/{accountId}/unapproved/{cohortRef}");
        }

        [Test, MoqAutoData]
        public void Then_Uses_Folder_When_There_Is_A_Ukprn_And_Is_Empty_Cohort_Journey(
            string accountId,
            string hashedAccountLegalEntityId,
            uint ukprn,
            [Frozen] ReservationsWebConfiguration options,
            [Frozen] Mock<IConfiguration> config,
            ExternalUrlHelper urlHelper)
        {
            config.Setup(x => x["AuthType"]).Returns("employer");
            options.EmployerApprenticeUrl = $"https://{options.EmployerApprenticeUrl}";

            var actualUrl = urlHelper.GenerateCohortDetailsUrl(ukprn, accountId, string.Empty, true, string.Empty, hashedAccountLegalEntityId);

            actualUrl.Should().Be(
                $"{options.EmployerApprenticeUrl}/{accountId}/unapproved/add/assign?providerId={ukprn}&accountLegalEntityHashedId={hashedAccountLegalEntityId}");
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
            options.EmployerApprenticeUrl = $"https://{options.EmployerApprenticeUrl}";

            var actualUrl = urlHelper.GenerateCohortDetailsUrl(ukprn, accountId, string.Empty, true, journeyData);

            actualUrl.Should().Be(
                $"{options.EmployerApprenticeUrl}/{accountId}/unapproved/add/assign?" +
                         $"providerId={ukprn}&journeyData={journeyData}");
        }
    }
}
