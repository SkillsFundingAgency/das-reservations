using System.ComponentModel.DataAnnotations;

namespace SFA.DAS.Reservations.Web.Models
{
    public class ApprenticeshipTrainingFormModel
    {
        [Required(ErrorMessage = "Select a start date")]
        public string StartDate { get; set; }
        
        public string SelectedCourseId { get; set; }
    }
}