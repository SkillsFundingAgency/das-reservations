using System.Collections.Generic;

namespace SFA.DAS.Reservations.Web.Models
{
    public class ApprenticeshipTrainingViewModel : SelectCourseViewModel
    {
        public IEnumerable<StartDateViewModel> PossibleStartDates { get; set; }
        public string RouteName { get; set; }
        public string TrainingStartDate { get; set; }
       
        public bool IsProvider { get; set; }
        public string EmployersChosenCourse { get; set; }
    }
}