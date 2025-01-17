using System;

namespace SFA.DAS.Reservations.Domain.Interfaces
{
    public interface IExternalUrlHelper
    {
        /// <summary>
        /// usage https://subDomain.baseUrl/folder/id/controller/action?queryString
        /// </summary>
        /// <param name="urlParameters"></param>
        /// <returns></returns>
        string GenerateUrl(UrlParameters urlParameters);

        /// <summary>
        /// usage https://subDomain.baseUrl/folder/id/controller/action?queryString
        /// </summary>
        /// <param name="urlParameters"></param>
        /// <returns></returns>
        string GenerateAddApprenticeUrl(UrlParameters urlParameters);
        string GenerateAddApprenticeUrl(Guid? reservationId, string accountLegalEntityPublicHashedId, string courseId,
            uint? ukPrn, DateTime? startDate, string cohortRef, string accountHashedId,
            bool isEmptyEmployerCohort = false, string transferSenderId = "", string encodedPledgeApplicationId = "",
            string journeyData = "", Guid? apprenticeshipSessionKey = null, bool? beforeProviderSelected = null);
        string GenerateCohortDetailsUrl(uint? ukprn, string accountId, string cohortRef, bool isEmptyCohort = false,
            string journeyData = "", string accountLegalEntityHashedId = "", Guid? apprenticeshipSessionKey = null);
        string GenerateDashboardUrl(string accountId = null);
        string GenerateConfirmEmployerUrl(uint ukprn, string employerAccountLegalEntityPublicHashedId);
    }
}
