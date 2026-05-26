using SFA.DAS.Common.Domain.Types;
using System;
using System.ComponentModel.DataAnnotations;

namespace SFA.DAS.Reservations.Web.Models
{
    public class CompletedViewModel 
    {
        [Required(ErrorMessage = "Select what you would like to do next")]
        public CompletedReservationWhatsNext? WhatsNext { get; set; }
        public string TrainingDateDescription { get; set; }
        public string CourseDescription { get; set; }
        public LearningType? LearningType { get; set; } 
        public string AccountLegalEntityName { get; set; }
        public DateTime StartDate { get; set; }
        public string CourseId { get; set; }
        public uint? UkPrn { get; set; }
        public string CohortRef { get; set; }
        public string JourneyData { get; set; }
        public bool? UseLearnerData { get; set; }
        public string EmployerAccountId { get; set; }
        public string PublicHashedEmployerAccountId { get; set; }
        public Guid? Id { get; set; }
        public string AccountLegalEntityPublicHashedId { get; set; }

        public string FindApprenticeshipTrainingUrl { get; set; }

        public string AddAnApprenticeUrl { get; set; }

        public string HomepageUrl { get; set; }

        public string RecruitUrl { get; set; }
    }

    public enum CompletedReservationWhatsNext
    {
        Homepage,
        RecruitAnApprentice,
        AddAnApprentice,
        FindApprenticeshipTraining
    }
}