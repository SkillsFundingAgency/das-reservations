using System;
using SFA.DAS.Reservations.Domain.Courses;

namespace SFA.DAS.Reservations.Domain.Reservations
{
    public class Reservation
    {
        public Guid Id { get; set; }
        public string AccountLegalEntityName { get; set; }
        public long AccountLegalEntityId { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public Course Course { get; set; }
        public ReservationStatus Status { get; set; }
        public uint? ProviderId { get; set; }
    }
}