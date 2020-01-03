using System;

namespace SFA.DAS.Reservations.Application.Exceptions
{
    public class EmployerAgreementNotSignedException : Exception
    {
        public long AccountId;
        public long AccountLegalEntityId;
        public EmployerAgreementNotSignedException(long accountId, long accountLegalEntityId)
        {
            AccountId = accountId;
            AccountLegalEntityId = accountLegalEntityId;
        }
    }
}