using System;
using System.Collections.Generic;
using System.Text;
using SFA.DAS.Reservations.Domain.Courses;

namespace SFA.DAS.Reservations.Domain.Rules
{
    public enum AccountRestriction
    {
        All = 0,
        NonLevy = 1,
        Levy = 2,
        Account = 3
    }

    public class ReservationRule
    {
        public long Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ActiveFrom { get;set;  }
        public DateTime ActiveTo { get; set; }
        public AccountRestriction Restriction { get; set; }
        public Course Course { get; set;  }
    }
}
