using System;
using System.Collections.Generic;

namespace SFA.DAS.Reservations.Web.Models
{
    public class ApprenticeshipTrainingViewModel : SelectCourseViewModel
    {
        public Guid ReservationId { get; set; }
        public IEnumerable<StartDateViewModel> PossibleStartDates { get; set; }
        public string RouteName { get; set; }
        public string TrainingStartDate { get; set; }
       
    }
}