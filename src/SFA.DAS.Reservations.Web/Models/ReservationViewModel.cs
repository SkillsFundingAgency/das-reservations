using System;

namespace SFA.DAS.Reservations.Web.Models
{
    public class ReservationViewModel
    {
        public Guid Id { get; set; }
        public string Status { get; set; }
        public string LegalEntityName { get; set; }
        public string CourseName { get; set; }
        public string StartDate { get; set; }
    }
}
