using System;

namespace SFA.DAS.Reservations.Application.Exceptions
{
    public class FailedExpressionOfInterestValidationException : Exception
    {
        public long AccountId { get; }

        public FailedExpressionOfInterestValidationException(long accountId)
        {
            AccountId = accountId;
        }
    }
}