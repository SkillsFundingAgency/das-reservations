using System;

namespace SFA.DAS.Reservations.Domain.Reservations.Api
{
    public class ReservationApiRequest : BaseApiRequest
    {
        private readonly Func<string, long> _decodeFunc;
        private readonly string _hashedAccountId;
        
        public ReservationApiRequest(
            string baseUrl, 
            Func<string, long> decodeFunc, 
            string hashedAccountId, 
            DateTime startDate, 
            Guid id) 
            : base(baseUrl)
        {
            _decodeFunc = decodeFunc;
            _hashedAccountId = hashedAccountId;
            Id = id;
            StartDate = startDate.ToString("yyyy-MMM-dd");
        }

        public Guid Id { get; }

        public long AccountId => _decodeFunc(_hashedAccountId);

        public string StartDate { get; }

        public override string CreateUrl => $"{BaseUrl}api/accounts/{AccountId}/reservations";
        public override string GetUrl => $"{BaseUrl}api/reservations/{Id}";
    }

}