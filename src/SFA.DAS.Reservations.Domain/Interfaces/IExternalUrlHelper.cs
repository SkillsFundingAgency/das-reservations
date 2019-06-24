using System;

namespace SFA.DAS.Reservations.Domain.Interfaces
{
    public interface IExternalUrlHelper
    {
        string GenerateUrl(UrlParameters urlParameters);
        string GenerateAddApprenticeUrl(uint? ukPrn, Guid reservationId, string accountLegalEntityPublicHashedId, DateTime startDate, string courseId);
    }
}
