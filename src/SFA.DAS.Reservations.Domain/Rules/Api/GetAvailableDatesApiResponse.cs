using System;
using System.Collections.Generic;

namespace SFA.DAS.Reservations.Domain.Rules.Api
{
    public class GetAvailableDatesApiResponse
    {
        public IList<DateTime> AvailableDates { get; set; }
    }
}