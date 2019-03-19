namespace SFA.DAS.Reservations.Web.Models
{
    public class ReviewViewModel
    {
        public string RouteName { get; set; }
        public ReservationsRouteModel RouteModel { get; set; }
        public string StartDateDescription { get; set; }
        public string CourseDescription { get; set; }
        public string AccountLegalEntityName { get; set; }
    }
}