using System.ComponentModel.DataAnnotations;

namespace SFA.DAS.Reservations.Web.Models
{
    public class DeleteCompletedViewModel
    {
        [Required(ErrorMessage = "Select what you want to do next")]
        public bool? Manage { get; set; }
    }
}