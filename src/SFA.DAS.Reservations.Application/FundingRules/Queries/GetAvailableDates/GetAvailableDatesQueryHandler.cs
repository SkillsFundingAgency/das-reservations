using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.Reservations.Domain.Interfaces;

namespace SFA.DAS.Reservations.Application.FundingRules.Queries.GetAvailableDates
{
    public class GetAvailableDatesQueryHandler :IRequestHandler<GetAvailableDatesQuery,GetAvailableDatesResult>
    {
        private readonly IFundingRulesService _fundingRulesService;

        public GetAvailableDatesQueryHandler(IFundingRulesService fundingRulesService)
        {
            _fundingRulesService = fundingRulesService;
        }

        public async Task<GetAvailableDatesResult> Handle(GetAvailableDatesQuery request, CancellationToken cancellationToken)
        {
            var availableDates = await _fundingRulesService.GetAvailableDates(request.AccountLegalEntityId);

            return new GetAvailableDatesResult
            {
                AvailableDates = availableDates.AvailableDates
            };
        }
    }
}
