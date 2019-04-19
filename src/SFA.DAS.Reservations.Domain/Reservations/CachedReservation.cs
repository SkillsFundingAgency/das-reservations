using System;

namespace SFA.DAS.Reservations.Domain.Reservations
{
    public class CachedReservation
    {
        public Guid Id { get; set; }
        public long AccountId { get; set; }
        public long AccountLegalEntityId { get; set; }
        public string AccountLegalEntityName { get; set; }
        public string AccountLegalEntityPublicHashedId { get; set; }
        public string StartDate { get; set; }
        public string StartDateDescription { get; set; }
        public string CourseId { get; set; }
        public string CourseDescription { get; set; }
        public uint UkPrn { get; set; }
        public string AccountName { get; set; }
    }
}