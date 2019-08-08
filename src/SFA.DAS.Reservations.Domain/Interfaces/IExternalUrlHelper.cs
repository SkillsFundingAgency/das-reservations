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
        string GenerateAddApprenticeUrl(Guid reservationId, string accountLegalEntityPublicHashedId, string courseId, uint? ukPrn, DateTime? startDate, string cohortRef, string accountHashedId);

        /// <summary>
        /// usage https://subDomain.baseUrl/folder/id/controller/action?queryString
        /// </summary>
        /// <param name="urlParameters"></param>
        /// <returns></returns>
        string GenerateCommitmentsUrl(UrlParameters urlParameters);
    }
}
