using System.ComponentModel.DataAnnotations;

namespace SFA.DAS.Reservations.Web.Models
{
    public class DeleteCompletedViewModel
    {
        [Required(ErrorMessage = "Please choose what's next")]
        public bool? Manage { get; set; }
    }
}