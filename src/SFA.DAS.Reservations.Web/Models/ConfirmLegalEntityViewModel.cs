using System.ComponentModel.DataAnnotations;

namespace SFA.DAS.Reservations.Web.Models
{
    public class ConfirmLegalEntityViewModel
    {
        [Required(ErrorMessage = "Todo: need this error message for model validation")]
        public string LegalEntity { get; set; }
    }
}