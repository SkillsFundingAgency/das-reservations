using System;

namespace SFA.DAS.Reservations.Domain.Interfaces
{
    public interface IExternalUrlHelper
    {
        string GenerateUrl(UrlParameters urlParameters);
        string GenerateAddApprenticeUrl(UrlParameters urlParameters);
        string GenerateAddApprenticeUrl(Guid reservationId, string accountLegalEntityPublicHashedId, string courseId, uint ukPrn, DateTime? startDate, string cohortRef);
    }
}
