using System;

namespace SFA.DAS.Reservations.Web.Exceptions
{
    public class AccountLegalEntityInvalidException : Exception
    {
        public AccountLegalEntityInvalidException(string message): base (message)
        {
            
        }
    }
}
