using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace SFA.DAS.Reservations.Web.Models
{
    public class ApprenticeshipTrainingFormModel
    {
        [Required(ErrorMessage = "You must select a start date")]
        public string StartDate { get; set; }
        public string SelectedCourseId { get; set; }
        public string AccountLegalEntityPublicHashedId { get; set; }
        public string CohortRef { get; set; }
        public bool? FromReview { get; set; }
    }
}