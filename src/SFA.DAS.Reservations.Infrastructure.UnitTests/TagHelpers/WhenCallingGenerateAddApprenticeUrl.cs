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
    public class WhenCallingGenerateAddApprenticeUrl
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
        public void Then_Uses_EmployerApprenticeUrl_And_Params_To_Build_Employer_Url(
            UrlParameters urlParameters,
            [Frozen] ReservationsWebConfiguration webConfig,
            [Frozen] Mock<IConfiguration> config,
            ExternalUrlHelper urlHelper)
        {
            config.Setup(x => x["AuthType"]).Returns("employer");

            var originalConfigUrl = webConfig.EmployerApprenticeUrl;
            webConfig.EmployerApprenticeUrl = $"https://{webConfig.EmployerApprenticeUrl}";

            var actualUrl = urlHelper.GenerateAddApprenticeUrl(urlParameters);
            var expectedUrl = $"https://{urlParameters.SubDomain}.{originalConfigUrl}/{urlParameters.Folder}/{urlParameters.Id}/{urlParameters.Controller}/{urlParameters.Action}{urlParameters.QueryString}";

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

            var actualUrl = urlHelper.GenerateAddApprenticeUrl(reservationId,
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
        public void Then_Uses_Unapproved_Controller_With_ApprenticeshipSessionKey_When_There_Is_A_ApprenticeshipSessionKey(
           Guid reservationId,
           string accountLegalEntityPublicHashedId,
           uint ukPrn,
           string accountHashedId,
           Guid apprenticeshipSessionKey,
           DateTime startDate,
           [Frozen] ReservationsWebConfiguration webConfig,
           [Frozen] Mock<IConfiguration> config,
           ExternalUrlHelper urlHelper)
        {
            config.Setup(x => x["AuthType"]).Returns("employer");

            var originalConfigUrl = webConfig.EmployerApprenticeUrl;
            webConfig.EmployerApprenticeUrl = $"https://{webConfig.EmployerApprenticeUrl}";

            var actualUrl = urlHelper.GenerateAddApprenticeUrl(reservationId,
                accountLegalEntityPublicHashedId,
                null,
                ukPrn,
                startDate,
                "",
                accountHashedId,
                true,
                apprenticeshipSessionKey: apprenticeshipSessionKey);

            var expectedUrl = $"https://{originalConfigUrl}/{accountHashedId}/unapproved/add/apprentice?reservationId={reservationId}" +
                $"&accountLegalEntityHashedId={accountLegalEntityPublicHashedId}" +
                $"&providerId={ukPrn}" +               
                $"&startMonthYear={startDate:MMyyyy}" +
                $"&apprenticeshipSessionKey={apprenticeshipSessionKey}";

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

            var actualUrl = urlHelper.GenerateAddApprenticeUrl(reservationId,
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
        public void Then_Uses_Unapproved_Add_Apprentice_For_Select_Journey_With_Empty_Cohort_and_No_CourseId(
            Guid reservationId,
            string accountLegalEntityPublicHashedId,
            string accountHashedId,
            uint ukPrn,
            DateTime startDate,
            [Frozen] ReservationsWebConfiguration webConfig,
            [Frozen] Mock<IConfiguration> config,
            ExternalUrlHelper urlHelper)
        {
            config.Setup(x => x["AuthType"]).Returns("employer");

            var originalConfigUrl = webConfig.EmployerApprenticeUrl;
            webConfig.EmployerApprenticeUrl = $"https://{webConfig.EmployerApprenticeUrl}";

            var actualUrl = urlHelper.GenerateAddApprenticeUrl(reservationId,
                accountLegalEntityPublicHashedId,
                null,
                ukPrn,
                startDate,
                "",
                accountHashedId,
                true);

            var expectedUrl = $"https://{originalConfigUrl}/{accountHashedId}/unapproved/add/apprentice?reservationId={reservationId}&accountLegalEntityHashedId={accountLegalEntityPublicHashedId}&providerId={ukPrn}&startMonthYear={startDate:MMyyyy}";

            actualUrl.Should().Be(expectedUrl);
        }

        [Test, MoqAutoData]
        public void Then_Uses_Unapproved_SelectDeliveryModel_For_Select_Journey_With_CourseId_And_Empty_Cohort(
           Guid reservationId,
           string accountLegalEntityPublicHashedId,
           string accountHashedId,
           string courseId,
           uint ukPrn,
           DateTime startDate,
           [Frozen] ReservationsWebConfiguration webConfig,
           [Frozen] Mock<IConfiguration> config,
           ExternalUrlHelper urlHelper)
        {
            config.Setup(x => x["AuthType"]).Returns("employer");

            var originalConfigUrl = webConfig.EmployerApprenticeUrl;
            webConfig.EmployerApprenticeUrl = $"https://{webConfig.EmployerApprenticeUrl}";

            var actualUrl = urlHelper.GenerateAddApprenticeUrl(reservationId,
                accountLegalEntityPublicHashedId,
                courseId,
                ukPrn,
                startDate,
                "",
                accountHashedId,
                true);

            var expectedUrl = $"https://{originalConfigUrl}/{accountHashedId}/unapproved/add/select-delivery-model?reservationId={reservationId}&accountLegalEntityHashedId={accountLegalEntityPublicHashedId}&providerId={ukPrn}&startMonthYear={startDate:MMyyyy}&courseCode={courseId}";

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

            var actualUrl = urlHelper.GenerateAddApprenticeUrl(reservationId,
                accountLegalEntityPublicHashedId,
                courseId,
                ukPrn,
                startDate,
                "",
                "");

            var expectedUrl = $"https://{originalConfigUrl}/{ukPrn}/unapproved/add/apprentice?reservationId={reservationId}&employerAccountLegalEntityPublicHashedId={accountLegalEntityPublicHashedId}&startMonthYear={startDate:MMyyyy}&courseCode={courseId}";
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

            var actualUrl = urlHelper.GenerateAddApprenticeUrl(reservationId,
                accountLegalEntityPublicHashedId,
                "",
                ukPrn,
                null,
                "",
                "");

            var expectedUrl = $"https://{originalConfigUrl}/{ukPrn}/unapproved/add/apprentice?reservationId={reservationId}&employerAccountLegalEntityPublicHashedId={accountLegalEntityPublicHashedId}&autocreated=true";
            actualUrl.Should().Be(expectedUrl);
        }

        [Test, MoqAutoData]
        public void Then_adds_levy_flag_To_query_string_if_no_course_or_start_date_provided(
            Guid reservationId,
            string accountLegalEntityPublicHashedId,
            string accountHashedId,
            [Frozen] ReservationsWebConfiguration webConfig,
            [Frozen] Mock<IConfiguration> config,
            ExternalUrlHelper urlHelper)
        {
            config.Setup(x => x["AuthType"]).Returns("employer");

            var originalConfigUrl = webConfig.EmployerApprenticeUrl;
            webConfig.EmployerApprenticeUrl = $"https://{webConfig.EmployerApprenticeUrl}";

            var actualUrl = urlHelper.GenerateAddApprenticeUrl(reservationId,
                accountLegalEntityPublicHashedId,
                "",
                null,
                null,
                "",
                accountHashedId);

            var expectedUrl = $"https://{originalConfigUrl}/{accountHashedId}/unapproved/add?reservationId={reservationId}&accountLegalEntityHashedId={accountLegalEntityPublicHashedId}&autocreated=true";
            actualUrl.Should().Be(expectedUrl);
        }

        [Test, MoqAutoData]
        public void Then_Adds_The_TransferSenderId_If_It_Has_Been_Provided(
            Guid reservationId,
            string accountLegalEntityPublicHashedId,
            string courseId,
            string transferSenderId,
            uint ukPrn,
            DateTime startDate,
            [Frozen] ReservationsWebConfiguration webConfig,
            [Frozen] Mock<IConfiguration> config,
            ExternalUrlHelper urlHelper)
        {
            config.Setup(x => x["AuthType"]).Returns("provider");

            var originalConfigUrl = webConfig.ApprenticeUrl;
            webConfig.ApprenticeUrl = $"https://{webConfig.ApprenticeUrl}";

            var actualUrl = urlHelper.GenerateAddApprenticeUrl(reservationId,
                accountLegalEntityPublicHashedId,
                courseId,
                ukPrn,
                startDate,
                "",
                "",
                false,
                transferSenderId);

            var expectedUrl = $"https://{originalConfigUrl}/{ukPrn}/unapproved/add/apprentice?reservationId={reservationId}&employerAccountLegalEntityPublicHashedId={accountLegalEntityPublicHashedId}&startMonthYear={startDate:MMyyyy}&courseCode={courseId}&transferSenderId={transferSenderId}";
            actualUrl.Should().Be(expectedUrl);
        }

        [Test, MoqAutoData]
        public void Then_Uses_Unapproved_SetReservation_For_Select_Journey_With_CourseId_And_Empty_Cohort(
            Guid reservationId,
            string accountLegalEntityPublicHashedId,
            string accountHashedId,
            string courseId,
            uint ukPrn,
            DateTime startDate,
            [Frozen] ReservationsWebConfiguration webConfig,
            [Frozen] Mock<IConfiguration> config,
            ExternalUrlHelper urlHelper)
        {
            config.Setup(x => x["AuthType"]).Returns("employer");

            var originalConfigUrl = webConfig.EmployerApprenticeUrl;
            webConfig.EmployerApprenticeUrl = $"https://{webConfig.EmployerApprenticeUrl}";

            var actualUrl = urlHelper.GenerateAddApprenticeUrl(reservationId,
                accountLegalEntityPublicHashedId,
                courseId,
                null,
                startDate,
                "",
                accountHashedId,
                true, 
                null,
                null,
                null,
                null,
                true);

            var expectedUrl = $"https://{originalConfigUrl}/{accountHashedId}/unapproved/add/set-reservation?reservationId={reservationId}&accountLegalEntityHashedId={accountLegalEntityPublicHashedId}&startMonthYear={startDate:MMyyyy}&courseCode={courseId}";

            actualUrl.Should().Be(expectedUrl);
        }

    }
}