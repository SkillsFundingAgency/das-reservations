using System.ComponentModel.DataAnnotations;

namespace SFA.DAS.Reservations.Web.Models
{
    public class PostSelectCourseViewModel
    {
        public string SelectedCourseId { get; set; }
        [Required(ErrorMessage = "Select whether you know which apprenticeship training your apprentice will take")]
        public bool? ApprenticeTrainingKnown { get; set; }
    }
}
