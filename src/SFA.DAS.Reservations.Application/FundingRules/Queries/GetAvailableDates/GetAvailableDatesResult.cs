using System;
using System.Collections.Generic;

namespace SFA.DAS.Reservations.Application.FundingRules.Queries.GetAvailableDates
{
    public class GetAvailableDatesResult
    {
        public IList<DateTime> AvailableDates { get; set; }
    }
}