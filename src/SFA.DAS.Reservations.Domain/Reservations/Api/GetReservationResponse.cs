using System;
using SFA.DAS.Reservations.Domain.Courses;

namespace SFA.DAS.Reservations.Domain.Reservations.Api
{
    public class GetReservationResponse
    {
        public Guid Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public Course Course { get; set; }
        public string AccountLegalEntityName { get; set; }
        public long AccountLegalEntityId { get; set; }
        public uint ProviderId { get; set; }
    }
}