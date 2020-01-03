using System.Collections.Generic;

namespace SFA.DAS.Reservations.Web.Models
{
    public class SignAgreementViewModel
    {
        public string BackRouteName { get; set; }
        public IEnumerable<EmployerAccountUserViewModel> OwnersOfThisAccount { get; set; }
        public bool IsUrl { get ; set ; }
    }
}