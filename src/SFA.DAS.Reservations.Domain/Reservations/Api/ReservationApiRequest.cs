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
            DateTime startDate, Guid id) 
            : base(baseUrl)
        {
            _decodeFunc = decodeFunc;
            _hashedAccountId = hashedAccountId;
            _id = id;
            StartDate = startDate;
        }

        public long AccountId => _decodeFunc(_hashedAccountId);

        public DateTime StartDate { get; }

        public override string CreateUrl => $"{BaseUrl}api/accounts/{AccountId}/reservations";
        public override string GetUrl => $"{BaseUrl}api/accounts/{AccountId}/reservations/{_id}";
    }

}