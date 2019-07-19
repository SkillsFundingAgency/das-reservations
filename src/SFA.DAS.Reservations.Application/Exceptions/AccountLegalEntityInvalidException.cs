using System;

namespace SFA.DAS.Reservations.Application.Exceptions
{
    public class AccountLegalEntityInvalidException : Exception
    {
        public AccountLegalEntityInvalidException(string message): base (message)
        {
            
        }
    }
}
