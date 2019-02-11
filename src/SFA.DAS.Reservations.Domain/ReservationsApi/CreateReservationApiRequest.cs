using System;

namespace SFA.DAS.Reservations.Domain.ReservationsApi
{
    public class CreateReservationApiRequest : BaseApiRequest
    {
        private readonly Func<string, long> _decodeFunc;
        private readonly string _hashedAccountId;

        public CreateReservationApiRequest(
            string url, 
            Func<string, long> decodeFunc, 
            string hashedAccountId, 
            DateTime startDate) 
            : base(url)
        {
            _decodeFunc = decodeFunc;
            _hashedAccountId = hashedAccountId;
            StartDate = startDate;
        }

        public long AccountId => _decodeFunc(_hashedAccountId);

        public DateTime StartDate { get; }
    }

    public abstract class BaseApiRequest
    {
        protected BaseApiRequest(string url)
        {
            Url = url;
        }

        public string Url { get; }
    }
}