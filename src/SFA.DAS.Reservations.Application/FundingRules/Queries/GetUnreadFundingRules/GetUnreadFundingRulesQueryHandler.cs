using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.Reservations.Domain.Interfaces;

namespace SFA.DAS.Reservations.Application.FundingRules.Queries.GetUnreadFundingRules
{
    public class GetUnreadFundingRulesQueryHandler : IRequestHandler<GetUnreadFundingRulesQuery, GetUnreadFundingRulesResult>
    {
        private readonly IFundingRulesService _service;

        public GetUnreadFundingRulesQueryHandler(IFundingRulesService service)
        {
            _service = service;
        }

        public async Task<GetUnreadFundingRulesResult> Handle(GetUnreadFundingRulesQuery request, CancellationToken cancellationToken)
        {
            var response = await _service.GetUnreadFundingRules(request.Id);

            return new GetUnreadFundingRulesResult
            {
                FundingRules = response
            };
        }
    }
}
