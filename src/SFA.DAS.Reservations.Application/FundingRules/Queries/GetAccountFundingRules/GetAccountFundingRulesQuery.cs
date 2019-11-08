using MediatR;

namespace SFA.DAS.Reservations.Application.FundingRules.Queries.GetAccountFundingRules
{
    public class GetAccountFundingRulesQuery : IRequest<GetAccountFundingRulesResult>
    {
        public long AccountId { get; set; }
        
    }
}
