using System;

namespace SFA.DAS.Reservations.Web.Exceptions
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
