using System;
using System.ComponentModel.DataAnnotations;

namespace SFA.DAS.Reservations.Web.Models
{
    public class PostReviewViewModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string CourseDescription { get; set; }
        public string AccountLegalEntityName { get; set; }
        public string AccountLegalEntityPublicHashedId { get; set; }
        [Required]
        public bool? Reserve { get; set; }
    }
}