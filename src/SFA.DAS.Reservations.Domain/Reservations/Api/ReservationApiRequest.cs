using System;
using SFA.DAS.Reservations.Domain.Interfaces;

namespace SFA.DAS.Reservations.Domain.Reservations.Api
{
    public class ReservationApiRequest : IGetApiRequest, IPostApiRequest
    {
        public ReservationApiRequest(
            string baseUrl,
            long accountId, 
            DateTime startDate, 
            Guid id, 
            string accountLegalEntityName,
            string courseId = null) 
        {
            BaseUrl = baseUrl;
            AccountId = accountId;
            Id = id;
            AccountLegalEntityName = accountLegalEntityName;
            StartDate = startDate.ToString("yyyy-MMM-dd");
            CourseId = courseId;
        }

        public ReservationApiRequest(string baseUrl, Guid id)
        {
            BaseUrl = baseUrl;
            Id = id;
        }

        public Guid Id { get; }

        public long AccountId { get; }

        public string StartDate { get; }

        public string CourseId { get; }
        public string BaseUrl { get; }
        public string CreateUrl => $"{BaseUrl}api/accounts/{AccountId}/reservations";
        public string GetUrl => $"{BaseUrl}api/reservations/{Id}";
        public string AccountLegalEntityName { get;}
    }

}