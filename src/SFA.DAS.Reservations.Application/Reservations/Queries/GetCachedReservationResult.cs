using System;

namespace SFA.DAS.Reservations.Application.Reservations.Queries
{
    public class GetCachedReservationResult
    {
        public Guid Id { get; set; }
        public string StartDate { get; set; }
        public string AccountId { get; set; }
        public string CourseId { get; set; }
    }
}