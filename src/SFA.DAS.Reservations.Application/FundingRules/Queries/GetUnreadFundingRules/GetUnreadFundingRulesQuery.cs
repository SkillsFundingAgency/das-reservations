using MediatR;
using SFA.DAS.Reservations.Application.FundingRules.Queries.GetFundingRules;

namespace SFA.DAS.Reservations.Application.FundingRules.Queries.GetUnreadFundingRules
{
    public class GetUnreadFundingRulesQuery : IRequest<GetUnreadFundingRulesResult>
    {
        public string Id { get; set; }
    }
}
