using System;

namespace SFA.DAS.Reservations.Application.Exceptions
{
    public class NonEoiUserAccessDeniedException : Exception
    {
        public long AccountId { get; }

        public NonEoiUserAccessDeniedException(long accountId)
        {
            AccountId = accountId;
        }
    }
}
 