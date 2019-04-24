using System;
using SFA.DAS.Reservations.Domain.Interfaces;

namespace SFA.DAS.Reservations.Domain.Reservations.Api
{
    public class ReservationApiRequest : IGetApiRequest, IGetAllApiRequest, IPostApiRequest
    {
        public ReservationApiRequest(
            string baseUrl,
            long accountId, 
            uint providerId,
            DateTime startDate, 
            Guid id, 
            long legalEntityAccountId,
            string accountLegalEntityName,
            string courseId = null) 
        {
            BaseUrl = baseUrl;
            AccountId = accountId;
            ProviderId = providerId;
            Id = id;
            AccountLegalEntityId = legalEntityAccountId;
            AccountLegalEntityName = accountLegalEntityName;
            StartDate = startDate.ToString("yyyy-MMM-dd");
            CourseId = courseId;
        }

        public ReservationApiRequest(string baseUrl, Guid id)
        {
            BaseUrl = baseUrl;
            Id = id;
        }

        public ReservationApiRequest(string baseUrl, long accountId)
        {
            BaseUrl = baseUrl;
            AccountId = accountId;
        }

        public Guid Id { get; }

        public long AccountId { get; }
        public uint ProviderId { get; }

        public string StartDate { get; }

        public string CourseId { get; }
        public string BaseUrl { get; }
        public string CreateUrl => $"{BaseUrl}api/accounts/{AccountId}/reservations";
        public string GetUrl => $"{BaseUrl}api/reservations/{Id}";
        public string GetAllUrl => $"{BaseUrl}api/accounts/{AccountId}/reservations";

        public long AccountLegalEntityId { get;}
        public string AccountLegalEntityName { get;}
    }

}