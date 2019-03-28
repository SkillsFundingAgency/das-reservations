using System.Collections.Generic;

namespace SFA.DAS.Reservations.Web.Models
{
    public class ApprenticeshipTrainingViewModel
    {
        public IEnumerable<StartDateViewModel> PossibleStartDates { get; set; }
        public string RouteName { get; set; }
        public IEnumerable<CourseViewModel> Courses { get; set; }
        public string TrainingStartDate { get; set; }
        public string CourseId { get; set; }
        public bool IsProvider { get; set; }
        public string EmployersChosenCourse { get; set; }
    }
}