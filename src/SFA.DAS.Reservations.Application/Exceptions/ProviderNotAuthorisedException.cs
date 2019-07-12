using System;

namespace SFA.DAS.Reservations.Application.Exceptions
{
    public class ProviderNotAuthorisedException : Exception
    {
        public long AccountId { get; }
        public uint UkPrn { get; set; }

        public ProviderNotAuthorisedException(long accountId, uint ukPrn)
        {
            UkPrn = ukPrn;
            AccountId = accountId;
        }
    }
}
