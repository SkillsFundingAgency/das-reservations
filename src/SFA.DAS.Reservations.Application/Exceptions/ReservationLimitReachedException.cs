using System;
using System.Collections.Generic;
using System.Text;

namespace SFA.DAS.Reservations.Application.Exceptions
{
    public class ReservationLimitReachedException : Exception
    {
        public long AccountId { get; }

        public ReservationLimitReachedException(long accountId)
        {
            AccountId = accountId;
        }
    }
}
