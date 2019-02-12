using System;
using Newtonsoft.Json;

namespace SFA.DAS.Reservations.Domain.ReservationsApi
{
    public class CreateReservationApiRequest : BaseApiRequest
    {
        private readonly Func<string, long> _decodeFunc;
        private readonly string _hashedAccountId;

        public CreateReservationApiRequest(
            string baseUrl, 
            Func<string, long> decodeFunc, 
            string hashedAccountId, 
            DateTime startDate) 
            : base(baseUrl)
        {
            _decodeFunc = decodeFunc;
            _hashedAccountId = hashedAccountId;
            StartDate = startDate;
        }

        public long AccountId => _decodeFunc(_hashedAccountId);

        public DateTime StartDate { get; }

        public override string CreateUrl => $"{BaseUrl}api/accounts/{_hashedAccountId}/reservations";
        public override string GetUrl => throw new NotImplementedException();
    }
}