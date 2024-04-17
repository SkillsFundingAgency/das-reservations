using System;
using System.Collections.Generic;

namespace SFA.DAS.Reservations.Domain.Rules.Api
{
    public class GetAvailableDatesApiResponse
    {
        public IList<TrainingDateModel> AvailableDates { get; set; }
        public TrainingDateModel PreviousMonth { get; set; }
    }
}