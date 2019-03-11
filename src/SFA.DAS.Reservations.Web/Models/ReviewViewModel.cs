using System;

namespace SFA.DAS.Reservations.Web.Models
{
    public class ReviewViewModel
    {
        public string RouteName { get; set; }
        public ReservationsRouteModel RouteModel { get; set; }
        public DateTime StartDate { get; set; }
    }
}