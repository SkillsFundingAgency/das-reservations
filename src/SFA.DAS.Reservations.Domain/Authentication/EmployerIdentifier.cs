using System.Collections.Generic;
using SFA.DAS.EAS.Account.Api.Types;

namespace SFA.DAS.Reservations.Domain.Authentication
{
    public class EmployerIdentifier
    {
        public string AccountId { get; set; }
        public string EmployerName { get; set; }
        public string Role { get; set; }
        public List<ResourceViewModel> LegalEntityResources { get; set; }
    }
}