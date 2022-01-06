using SFA.DAS.Reservations.Domain.Rules;

namespace SFA.DAS.Reservations.Web.Models
{
    public class EmployerStartViewModel
    {
        public GlobalRule ActiveGlobalRule { get; set; }
        public string FindApprenticeshipTrainingUrl { get; set; }
        public string ApprenticeshipFundingRulesUrl { get; set; }
    }
}
