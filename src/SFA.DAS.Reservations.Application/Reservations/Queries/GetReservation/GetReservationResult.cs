using System;
using SFA.DAS.Reservations.Domain.Courses;

namespace SFA.DAS.Reservations.Application.Reservations.Queries.GetReservation
{
    public class GetReservationResult
    {
        public Guid ReservationId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public Course Course { get; set; }
    }
}