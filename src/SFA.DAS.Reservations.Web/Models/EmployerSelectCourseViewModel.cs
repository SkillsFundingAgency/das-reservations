using System;

namespace SFA.DAS.Reservations.Web.Models
{
    public class EmployerSelectCourseViewModel : SelectCourseViewModel
    {
        public Guid ReservationId { get; set; }
        public string RouteName { get; set; }
        public string BackLink { get; set; }
        public string CohortRef { get; set; }
    }
}
