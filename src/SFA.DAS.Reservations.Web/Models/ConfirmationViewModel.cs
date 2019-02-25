using System;

namespace SFA.DAS.Reservations.Web.Models
{
    public class ConfirmationViewModel
    {
        public Guid ReservationId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime ExpiryDate { get; set; }
    }
}