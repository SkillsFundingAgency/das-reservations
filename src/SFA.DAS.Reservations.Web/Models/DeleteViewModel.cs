using System;
using SFA.DAS.Reservations.Domain.Courses;

namespace SFA.DAS.Reservations.Web.Models
{
    public class DeleteViewModel
    {
        public Guid ReservationId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public Course Course { get; set; }
        public string AccountLegalEntityName { get; set; }
    }
}