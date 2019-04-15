using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.Reservations.Application.FundingRules.Services;

namespace SFA.DAS.Reservations.Application.FundingRules.Queries.GetFundingRules
{
    public class GetFundingRulesQueryHandler : IRequestHandler<GetFundingRulesQuery, GetFundingRulesResult>
    {
        private readonly IRulesService _service;

        public GetFundingRulesQueryHandler(IRulesService service)
        {
            _service = service;
        }

        public async Task<GetFundingRulesResult> Handle(GetFundingRulesQuery request, CancellationToken cancellationToken)
        {
            var response = await _service.GetFundingRules();

            return new GetFundingRulesResult
            {
                FundingRules = response
            };
        }
    }
}
