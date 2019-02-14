using System.Collections.Generic;

namespace SFA.DAS.Reservations.Web.Models
{
    public class ApprenticeshipTrainingViewModel
    {
        public IEnumerable<StartDateViewModel> PossibleStartDates { get; set; }
    }

    public class StartDateViewModel
    {
        public string Value { get; set; }
        public string Label { get; set; }
    }
}