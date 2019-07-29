using System;
using AutoFixture.NUnit3;
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
        public void Then_Uses_ApprenticeUrl_And_Params_To_Build_Url(
            UrlParameters urlParameters,
            [Frozen] ReservationsWebConfiguration config,
            ExternalUrlHelper urlHelper)
        {
            var originalConfigUrl = config.ApprenticeUrl;
            config.ApprenticeUrl = $"https://{config.ApprenticeUrl}";
            
            var actualUrl = urlHelper.GenerateAddApprenticeUrl(urlParameters);
            
            Assert.AreEqual(
                $"https://{urlParameters.SubDomain}.{originalConfigUrl}/{urlParameters.Folder}/{urlParameters.Id}/{urlParameters.Controller}/{urlParameters.Action}{urlParameters.QueryString}", 
                actualUrl);
        }

        [Test, MoqAutoData]
        public void Then_Uses_Unapproved_Controller_With_Cohort_Ref_When_There_Is_A_Cohort_Ref(
            Guid reservationId,
            string accountLegalEntityPublicHashedId,
            string courseId,
            uint ukPrn,
            DateTime startDate,
            string cohortRef,
            [Frozen] ReservationsWebConfiguration config,
            ExternalUrlHelper urlHelper)
        {
            var originalConfigUrl = config.ApprenticeUrl;
            config.ApprenticeUrl = $"https://{config.ApprenticeUrl}";

            var actualUrl = urlHelper.GenerateAddApprenticeUrl(reservationId,
                accountLegalEntityPublicHashedId,
                courseId,
                ukPrn,
                startDate,
                cohortRef);

            Assert.AreEqual(
                $"https://{originalConfigUrl}/{ukPrn}/unapproved/{cohortRef}/apprentices/add?reservationId={reservationId}&employerAccountLegalEntityPublicHashedId={accountLegalEntityPublicHashedId}&startMonthYear={startDate:MMyyyy}&courseCode={courseId}",
                actualUrl);
        }


        [Test, MoqAutoData]
        public void Then_Uses_AddApprentice_Action_With_No_Cohort_Ref_When_There_Is_No_Cohort_Ref(
            Guid reservationId,
            string accountLegalEntityPublicHashedId,
            string courseId,
            uint ukPrn,
            DateTime startDate,
            [Frozen] ReservationsWebConfiguration config,
            ExternalUrlHelper urlHelper)
        {
            var originalConfigUrl = config.ApprenticeUrl;
            config.ApprenticeUrl = $"https://{config.ApprenticeUrl}";

            var actualUrl = urlHelper.GenerateAddApprenticeUrl(reservationId,
                accountLegalEntityPublicHashedId,
                courseId,
                ukPrn,
                startDate,
                "");

            Assert.AreEqual(
                $"https://{originalConfigUrl}/{ukPrn}/unapproved/add-apprentice?reservationId={reservationId}&employerAccountLegalEntityPublicHashedId={accountLegalEntityPublicHashedId}&startMonthYear={startDate:MMyyyy}&courseCode={courseId}",
                actualUrl);
        }



        [Test, MoqAutoData]
        public void Then_CourseCode_And_StartDate_Are_Not_Added_To_Query_String_If_Not_Supplied(
            Guid reservationId,
            string accountLegalEntityPublicHashedId,
            uint ukPrn,
            [Frozen] ReservationsWebConfiguration config,
            ExternalUrlHelper urlHelper)
        {
            var originalConfigUrl = config.ApprenticeUrl;
            config.ApprenticeUrl = $"https://{config.ApprenticeUrl}";

            var actualUrl = urlHelper.GenerateAddApprenticeUrl(reservationId,
                accountLegalEntityPublicHashedId,
                "",
                ukPrn,
                null,
                "");

            Assert.AreEqual(
                $"https://{originalConfigUrl}/{ukPrn}/unapproved/add-apprentice?reservationId={reservationId}&employerAccountLegalEntityPublicHashedId={accountLegalEntityPublicHashedId}",
                actualUrl);
        }

    }
}