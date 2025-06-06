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
        public string AccountLegalEntityName { get; set; }
        public DateTime StartDate { get; set; }
        public string CourseId { get; set; }
        public uint? UkPrn { get; set; }
        public string CohortRef { get; set; }
        public string JourneyData { get; set; }
        public bool? UseLearnerData { get; set; }
    }

    public enum CompletedReservationWhatsNext
    {
        Homepage,
        RecruitAnApprentice,
        AddAnApprentice,
        FindApprenticeshipTraining
    }
}