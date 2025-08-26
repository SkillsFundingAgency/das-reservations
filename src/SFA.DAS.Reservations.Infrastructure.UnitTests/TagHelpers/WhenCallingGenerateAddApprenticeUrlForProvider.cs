using System;
using AutoFixture.NUnit3;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Infrastructure.Configuration;
using SFA.DAS.Reservations.Infrastructure.TagHelpers;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Reservations.Infrastructure.UnitTests.TagHelpers
{
    [TestFixture]
    public class WhenCallingGenerateAddApprenticeUrlForProvider
    {
        [Test, MoqAutoData]
        public void Then_Uses_ApprenticeUrl_And_Params_To_Build_Provider_Url(
            UrlParameters urlParameters,
            [Frozen] ReservationsWebConfiguration webConfig,
            [Frozen] Mock<IConfiguration> config,
            ExternalUrlHelper urlHelper)
        {
            config.Setup(x => x["AuthType"]).Returns("provider");

            var originalConfigUrl = webConfig.ApprenticeUrl;
            webConfig.ApprenticeUrl = $"https://{webConfig.ApprenticeUrl}";

            var actualUrl = urlHelper.GenerateAddApprenticeUrl(urlParameters);
            var expectedUrl = $"https://{urlParameters.SubDomain}.{originalConfigUrl}/{urlParameters.Folder}/{urlParameters.Id}" +
                         $"/{urlParameters.Controller}/{urlParameters.Action}{urlParameters.QueryString}";

            actualUrl.Should().Be(expectedUrl);
        }

        [Test, MoqAutoData]
        public void Then_Uses_Unapproved_Controller_With_Cohort_Ref_When_There_Is_A_Cohort_Ref(
            Guid reservationId,
            string accountLegalEntityPublicHashedId,
            string courseId,
            uint ukPrn,
            DateTime startDate,
            string cohortRef,
            [Frozen] ReservationsWebConfiguration webConfig,
            [Frozen] Mock<IConfiguration> config,
            ExternalUrlHelper urlHelper)
        {
            config.Setup(x => x["AuthType"]).Returns("provider");

            var originalConfigUrl = webConfig.ApprenticeUrl;
            webConfig.ApprenticeUrl = $"https://{webConfig.ApprenticeUrl}";

            var actualUrl = urlHelper.GenerateAddApprenticeUrlForProvider(reservationId,
                accountLegalEntityPublicHashedId,
                courseId,
                ukPrn,
                startDate,
                cohortRef,
                "");

            var expectedUrl = $"https://{originalConfigUrl}/{ukPrn}/unapproved/{cohortRef}/apprentices/add?" +
                         $"reservationId={reservationId}&employerAccountLegalEntityPublicHashedId={accountLegalEntityPublicHashedId}" +
                         $"&startMonthYear={startDate:MMyyyy}&courseCode={courseId}";

            actualUrl.Should().Be(expectedUrl);
        }

        [Test, MoqAutoData]
        public void Then_Uses_Journey_Data(
            Guid reservationId,
            string accountLegalEntityPublicHashedId,
            string courseId,
            uint ukPrn,
            DateTime startDate,
            string cohortRef,
            string journeyData,
            [Frozen] ReservationsWebConfiguration webConfig,
            [Frozen] Mock<IConfiguration> config,
            ExternalUrlHelper urlHelper)
        {
            config.Setup(x => x["AuthType"]).Returns("provider");

            var originalConfigUrl = webConfig.ApprenticeUrl;
            webConfig.ApprenticeUrl = $"https://{webConfig.ApprenticeUrl}";

            var actualUrl = urlHelper.GenerateAddApprenticeUrlForProvider(reservationId,
                accountLegalEntityPublicHashedId,
                courseId,
                ukPrn,
                startDate,
                cohortRef,
                "",
                journeyData: journeyData);

            var expectedUrl = $"https://{originalConfigUrl}/{ukPrn}/unapproved/{cohortRef}/apprentices/add?" +
                $"reservationId={reservationId}&employerAccountLegalEntityPublicHashedId={accountLegalEntityPublicHashedId}" +
                $"&startMonthYear={startDate:MMyyyy}&courseCode={courseId}&journeyData={journeyData}";

            actualUrl.Should().Be(expectedUrl);
        }


        [Test, MoqAutoData]
        public void Then_Uses_Add_Apprentice_Action_With_No_Cohort_Ref_When_There_Is_No_Cohort_Ref(
            Guid reservationId,
            string accountLegalEntityPublicHashedId,
            string courseId,
            uint ukPrn,
            DateTime startDate,
            [Frozen] ReservationsWebConfiguration webConfig,
            [Frozen] Mock<IConfiguration> config,
            ExternalUrlHelper urlHelper)
        {
            config.Setup(x => x["AuthType"]).Returns("provider");

            var originalConfigUrl = webConfig.ApprenticeUrl;
            webConfig.ApprenticeUrl = $"https://{webConfig.ApprenticeUrl}";

            var actualUrl = urlHelper.GenerateAddApprenticeUrlForProvider(reservationId,
                accountLegalEntityPublicHashedId,
                courseId,
                ukPrn,
                startDate,
                "",
                "");

            var expectedUrl = $"https://{originalConfigUrl}/{ukPrn}/unapproved/add-apprentice?reservationId={reservationId}&employerAccountLegalEntityPublicHashedId={accountLegalEntityPublicHashedId}&startMonthYear={startDate:MMyyyy}&courseCode={courseId}";
            actualUrl.Should().Be(expectedUrl);
        }

        [Test, MoqAutoData]
        public void And_IlrMode_Is_True_Then_Uses_Add_Select_Learner_Action_With_No_Cohort_Ref_When_There_Is_No_Cohort_Ref(
            Guid reservationId,
            string accountLegalEntityPublicHashedId,
            string courseId,
            uint ukPrn,
            DateTime startDate,
            [Frozen] ReservationsWebConfiguration webConfig,
            [Frozen] Mock<IConfiguration> config,
            ExternalUrlHelper urlHelper)
        {
            config.Setup(x => x["AuthType"]).Returns("provider");

            var originalConfigUrl = webConfig.ApprenticeUrl;
            webConfig.ApprenticeUrl = $"https://{webConfig.ApprenticeUrl}";

            var actualUrl = urlHelper.GenerateAddApprenticeUrlForProvider(reservationId,
                accountLegalEntityPublicHashedId,
                courseId,
                ukPrn,
                startDate,
                "",
                "",
                useLearnerData:true);

            var expectedUrl = $"https://{originalConfigUrl}/{ukPrn}/unapproved/add-apprentice?reservationId={reservationId}&employerAccountLegalEntityPublicHashedId={accountLegalEntityPublicHashedId}&startMonthYear={startDate:MMyyyy}&courseCode={courseId}&useLearnerData=true";
            actualUrl.Should().Be(expectedUrl);
        }

        [Test, MoqAutoData]
        public void Then_CourseCode_And_StartDate_Are_Not_Added_To_Query_String_If_Not_Supplied(
            Guid reservationId,
            string accountLegalEntityPublicHashedId,
            uint ukPrn,
            [Frozen] ReservationsWebConfiguration webConfig,
            [Frozen] Mock<IConfiguration> config,
            ExternalUrlHelper urlHelper)
        {
            config.Setup(x => x["AuthType"]).Returns("provider");

            var originalConfigUrl = webConfig.ApprenticeUrl;
            webConfig.ApprenticeUrl = $"https://{webConfig.ApprenticeUrl}";

            var actualUrl = urlHelper.GenerateAddApprenticeUrlForProvider(reservationId,
                accountLegalEntityPublicHashedId,
                "",
                ukPrn,
                null,
                "",
                "");

            var expectedUrl = $"https://{originalConfigUrl}/{ukPrn}/unapproved/add-apprentice?reservationId={reservationId}&employerAccountLegalEntityPublicHashedId={accountLegalEntityPublicHashedId}";
            actualUrl.Should().Be(expectedUrl);
        }
    }
}