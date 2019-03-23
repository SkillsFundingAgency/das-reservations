using System;
using SFA.DAS.Reservations.Application.Reservations.Commands;

namespace SFA.DAS.Reservations.Application.Reservations.Queries
{
    public class GetCachedReservationResult : ICreateReservationCommand
    {
        public Guid Id { get; set; }
        public long AccountId { get; set; }
        public long AccountLegalEntityId { get; set; }
        public string AccountLegalEntityName { get; set; }
        public string StartDate { get; set; }
        public string StartDateDescription { get; set; }
        public string CourseId { get; set; }
        public string CourseDescription { get; set; }
    }
}