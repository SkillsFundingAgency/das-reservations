namespace SFA.DAS.Reservations.Web.Models
{
    public class ReviewViewModel
    {
        public string ConfirmRouteName { get; set; }
        public string ChangeRouteName { get; set; }
        public ReservationsRouteModel RouteModel { get; set; }
        public string StartDateDescription { get; set; }
        public string CourseDescription { get; set; }
        public string EmployerDescription { get; set; }
    }
}