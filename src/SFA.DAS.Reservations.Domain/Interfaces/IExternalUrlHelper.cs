using System;

namespace SFA.DAS.Reservations.Domain.Interfaces
{
    public interface IExternalUrlHelper
    {
        string GenerateUrl(string id = "", string controller = "", string action = "", string subDomain = "", string folder = "", string queryString="");
        string GenerateAddApprenticeUrl(uint? ukPrn, Guid reservationId, string accountLegalEntityPublicHashedId, DateTime startDate, string courseId);
    }
}
