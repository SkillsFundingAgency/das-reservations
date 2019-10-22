using System;
using SFA.DAS.Reservations.Domain.Rules;

namespace SFA.DAS.Reservations.Domain.Reservations
{
    public class CachedReservation
    {
        public Guid Id { get; set; }
        public long AccountId { get; set; }
        public long AccountLegalEntityId { get; set; }
        public string AccountLegalEntityName { get; set; }
        public string AccountLegalEntityPublicHashedId { get; set; }
        public TrainingDateModel TrainingDate { get; set; }
        public string CourseId { get; set; }
        public string CourseDescription { get; set; }
        public uint? UkPrn { get; set; }
        public string AccountName { get; set; }
        public string CohortRef { get; set; }
        public bool EmployerHasSingleLegalEntity { get; set; }
        public bool IsEmptyCohortFromSelect { get; set; }
    }
}