using System.Collections.Generic;
using SFA.DAS.Reservations.Domain.Courses;

namespace SFA.DAS.Reservations.Web.Models
{
    public class ApprenticeshipTrainingViewModel
    {
        public IEnumerable<StartDateViewModel> PossibleStartDates { get; set; }
        public string RouteName { get; set; }
        public IEnumerable<Course> Courses { get; set; }
    }
}