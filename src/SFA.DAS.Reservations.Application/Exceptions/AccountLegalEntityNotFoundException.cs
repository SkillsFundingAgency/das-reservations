using System;

namespace SFA.DAS.Reservations.Application.Exceptions
{
    public class AccountLegalEntityNotFoundException : Exception
    {
        public string AccountLegalEntityPublicHashedId { get; }

        public AccountLegalEntityNotFoundException(string publicHashedId)
        {
            AccountLegalEntityPublicHashedId = publicHashedId;
        }
    }
}
