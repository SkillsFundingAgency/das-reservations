using System.Collections.Generic;
using SFA.DAS.Reservations.Domain.Employers;

namespace SFA.DAS.Reservations.Web.Models
{
    public class ChooseEmployerViewModel
    {
        public IEnumerable<AccountLegalEntity> Employers { get; set; }
    }
}
