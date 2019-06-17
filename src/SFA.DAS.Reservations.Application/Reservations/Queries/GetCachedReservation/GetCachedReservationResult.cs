using System;
using SFA.DAS.Reservations.Domain.Rules;

namespace SFA.DAS.Reservations.Application.Reservations.Queries.GetCachedReservation
{
    public class GetCachedReservationResult
    {
        public Guid Id { get; set; }
        public long AccountId { get; set; }
        public long AccountLegalEntityId { get; set; }
        public string AccountLegalEntityName { get; set; }
        public TrainingDateModel TrainingDate { get; set; }
        public string CourseId { get; set; }
        public string CourseDescription { get; set; }
        public string AccountLegalEntityPublicHashedId { get; set; }
        public string AccountName { get; set; }
    }
}