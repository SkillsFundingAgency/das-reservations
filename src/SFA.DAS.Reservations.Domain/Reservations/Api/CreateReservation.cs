using System;

namespace SFA.DAS.Reservations.Domain.Reservations.Api
{
    public class CreateReservation : BaseApiRequest
    {
        private readonly Func<string, long> _decodeFunc;
        private readonly string _hashedAccountId;

        public CreateReservation(
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

        public override string CreateUrl => $"{BaseUrl}api/accounts/{AccountId}/reservations";
        public override string GetUrl => throw new NotImplementedException();
    }
}