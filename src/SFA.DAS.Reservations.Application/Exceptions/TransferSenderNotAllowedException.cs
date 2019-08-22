using System;

namespace SFA.DAS.Reservations.Application.Exceptions
{
    public class TransferSenderNotAllowedException : Exception
    {
        public long AccountId;
        public string TransferSenderId;

        public TransferSenderNotAllowedException(long accountId, string transferSenderId)
        {
            AccountId = accountId;
            TransferSenderId = transferSenderId;
        }
    }
}