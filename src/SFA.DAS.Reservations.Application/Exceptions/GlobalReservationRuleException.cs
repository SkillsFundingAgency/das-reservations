using System;

namespace SFA.DAS.Reservations.Application.Exceptions
{
    public class GlobalReservationRuleException : Exception
    {
        public long AccountId;

        public GlobalReservationRuleException(long accountId)
        {
            AccountId = accountId;
        }
    }
}