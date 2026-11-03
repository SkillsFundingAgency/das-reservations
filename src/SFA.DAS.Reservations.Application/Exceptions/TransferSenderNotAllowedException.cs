using System;

namespace SFA.DAS.Reservations.Application.Exceptions
{
    public class TransferSenderNotAllowedException : Exception
    {
        public long AccountId;
        public string TransferSenderId;
        public string PledgeApplicationStatus;

        public TransferSenderNotAllowedException(long accountId, string transferSenderId, string pledgeApplicationStatus = null)
        {
            AccountId = accountId;
            TransferSenderId = transferSenderId;
            PledgeApplicationStatus = pledgeApplicationStatus;
        }
    }
}