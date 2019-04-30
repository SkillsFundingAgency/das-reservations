using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.Reservations.Application.FundingRules.Services;
using SFA.DAS.Reservations.Domain.Interfaces;

namespace SFA.DAS.Reservations.Application.FundingRules.Queries.GetFundingRules
{
    public class GetFundingRulesQueryHandler : IRequestHandler<GetFundingRulesQuery, GetFundingRulesResult>
    {
        private readonly IFundingRulesService _service;

        public GetFundingRulesQueryHandler(IFundingRulesService service)
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
