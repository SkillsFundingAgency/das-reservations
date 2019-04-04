using System;
using System.ComponentModel.DataAnnotations;

namespace SFA.DAS.Reservations.Web.Models
{
    public class ConfirmationRedirectViewModel
    {
        [Required(ErrorMessage = "Select whether to add an apprentice now or later")]
        public bool? AddApprentice { get; set; }
        public string DashboardUrl { get; set; }
        public string ApprenticeUrl { get; set; }
        public Guid ReservationId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string AccountLegalEntityName { get; set; }
        public string CourseId { get; set; }
        public int Level { get; set; }
        public string CourseTitle { get; set; }
    }
}