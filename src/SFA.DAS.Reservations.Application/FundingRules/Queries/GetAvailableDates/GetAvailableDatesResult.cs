using System;
using System.Collections.Generic;
using SFA.DAS.Reservations.Domain.Rules;

namespace SFA.DAS.Reservations.Application.FundingRules.Queries.GetAvailableDates
{
    public class GetAvailableDatesResult
    {
        public IList<StartDateModel> AvailableDates { get; set; }
    }
}