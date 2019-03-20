using System;

namespace SFA.DAS.Reservations.Domain.Reservations.Api
{
    public class ReservationApiRequest : BaseApiRequest
    {
        public ReservationApiRequest(
            string baseUrl, 
            long accountId, 
            DateTime startDate, 
            Guid id, 
            string courseId = null) 
            : base(baseUrl)
        {
            AccountId = accountId;
            Id = id;
            StartDate = startDate.ToString("yyyy-MMM-dd");
            CourseId = courseId;
        }

        //Todo: This ctor will be changed when we change the BaseRequestApi logic
        public ReservationApiRequest(string baseUrl,  Guid id) : base (baseUrl)
        {
            Id = id;
        }

        public Guid Id { get; }

        public long AccountId { get; }

        public string StartDate { get; }

        public string CourseId { get; }

        public override string CreateUrl => $"{BaseUrl}api/accounts/{AccountId}/reservations";
        public override string GetUrl => $"{BaseUrl}api/reservations/{Id}";
    }

}