using System;

namespace SFA.DAS.Reservations.Application.Exceptions
{
    public class TransferSendNotAllowedException : Exception
    {
        public long AccountId;
        public string TransferSenderId;

        public TransferSendNotAllowedException(long accountId, string transferSenderId)
        {
            AccountId = accountId;
            TransferSenderId = transferSenderId;
        }
    }
}