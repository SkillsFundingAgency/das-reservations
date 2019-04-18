using System.Collections.Generic;

namespace SFA.DAS.Reservations.Domain.Rules.Api
{
    public class GetFundingRulesApiResponse
    {
        public IEnumerable<ReservationRule> Rules { get; set; }
        public IEnumerable<GlobalRule> GlobalRules { get; set; }
    }
}
