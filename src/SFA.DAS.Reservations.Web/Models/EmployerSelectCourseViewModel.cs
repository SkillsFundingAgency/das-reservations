using System;

namespace SFA.DAS.Reservations.Web.Models
{
    public class EmployerSelectCourseViewModel : SelectCourseViewModel
    {
        public Guid ReservationId { get; set; }
        public string RouteName { get; set; }

    }
}
