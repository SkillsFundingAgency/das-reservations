using System;

namespace SFA.DAS.Reservations.Domain.Reservations.Api
{
    public class ReservationApiRequest : BaseApiRequest
    {
        private readonly Func<string, long> _decodeFunc;
        private readonly string _hashedAccountId;
        private readonly Guid _id;

        public ReservationApiRequest(
            string baseUrl, 
            Func<string, long> decodeFunc, 
            string hashedAccountId, 
            DateTime startDate, 
            Guid id, 
            string courseId = null) 
            : base(baseUrl)
        {
            _decodeFunc = decodeFunc;
            _hashedAccountId = hashedAccountId;
            _id = id;
            StartDate = startDate.ToString("yyyy-MMM-dd");
            CourseId = courseId;
        }

        public long AccountId => _decodeFunc(_hashedAccountId);

        public string StartDate { get; }

        public string CourseId { get; }

        public override string CreateUrl => $"{BaseUrl}api/accounts/{AccountId}/reservations";
        public override string GetUrl => $"{BaseUrl}api/reservations/{_id}";
    }

}