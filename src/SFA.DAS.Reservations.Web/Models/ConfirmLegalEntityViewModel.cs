using System.ComponentModel.DataAnnotations;

namespace SFA.DAS.Reservations.Web.Models
{
    public class ConfirmLegalEntityViewModel
    {
        [Required(ErrorMessage = "Select an organisation")]
        public string LegalEntity { get; set; }
    }
}