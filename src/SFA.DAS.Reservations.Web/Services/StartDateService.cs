using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.Reservations.Application.FundingRules.Queries.GetAvailableDates;
using SFA.DAS.Reservations.Domain.Rules;

namespace SFA.DAS.Reservations.Web.Services
{
    public class StartDateService : IStartDateService
    {
        private readonly IMediator _mediator;

        public StartDateService(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<IEnumerable<StartDateModel>> GetStartDates()
        {
            var datesToUse = await _mediator.Send(new GetAvailableDatesQuery());
            
            return await Task.FromResult(datesToUse.AvailableDates);
        }
    }
}