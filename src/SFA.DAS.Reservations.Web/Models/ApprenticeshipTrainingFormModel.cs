﻿using System.ComponentModel.DataAnnotations;

namespace SFA.DAS.Reservations.Web.Models
{
    public class ApprenticeshipTrainingFormModel
    {
        [Required(ErrorMessage = "You must select a start date")]
        public string StartDate { get; set; }
        [Required(ErrorMessage = "Select which apprenticeship training your apprentice will take")]
        public string SelectedCourseId { get; set; }
        public string AccountLegalEntityPublicHashedId { get; set; }
        public string CohortRef { get; set; }
        public bool? FromReview { get; set; }
    }
}