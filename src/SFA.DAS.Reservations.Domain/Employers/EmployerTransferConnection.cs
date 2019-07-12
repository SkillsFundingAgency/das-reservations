using System;
using System.Collections.Generic;
using System.Text;

namespace SFA.DAS.Reservations.Domain.Employers
{
    public class EmployerTransferConnection
    {
        public long FundingEmployerAccountId { get; set; }
        public string FundingEmployerHashedAccountId { get; set; }
        public string FundingEmployerPublicHashedAccountId { get; set; }
        public string FundingEmployerAccountName { get; set; }
    }
}
