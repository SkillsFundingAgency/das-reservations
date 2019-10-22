using System;

namespace SFA.DAS.Reservations.Web.Models
{
    public class EmployerSelectCourseViewModel : SelectCourseViewModel
    {
        public Guid ReservationId { get; set; }
        public string RouteName { get; set; }
        public string BackLink { get; set; }
        public string CohortReference { get; set; }
        public bool? ApprenticeTrainingKnown { get; set; }
        public bool IsEmptyCohortFromSelect { get; set; }
    }
}
