using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Domain.Rules;

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
               AccountRules = response?.Rules?.ToArray() ?? new ReservationRule[0],
               GlobalRules = response?.GlobalRules?.ToArray() ?? new GlobalRule[0]
            };
        }
    }
}
