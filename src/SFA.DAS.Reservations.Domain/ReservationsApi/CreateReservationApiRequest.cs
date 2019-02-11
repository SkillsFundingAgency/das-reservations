using System;

namespace SFA.DAS.Reservations.Domain.ReservationsApi
{
    public class CreateReservationApiRequest
    {
        private readonly Func<string, long> _decodeFunc;
        private readonly string _hashedAccountId;

        public CreateReservationApiRequest(Func<string, long> decodeFunc, string hashedAccountId, DateTime startDate)
        {
            _decodeFunc = decodeFunc;
            _hashedAccountId = hashedAccountId;
            StartDate = startDate;
        }

        public long AccountId => _decodeFunc(_hashedAccountId);

        public DateTime StartDate { get; }
    }
}