using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.Reservations.Application.FundingRules.Queries.GetAvailableDates;
using SFA.DAS.Reservations.Domain.Rules;

namespace SFA.DAS.Reservations.Web.Services
{
    public class TrainingDateService : ITrainingDateService
    {
        private readonly IMediator _mediator;

        public TrainingDateService(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<IEnumerable<TrainingDateModel>> GetTrainingDates(long accountLegalEntityId)
        {
            var datesToUse = await _mediator.Send(new GetAvailableDatesQuery
                {AccountLegalEntityId = accountLegalEntityId});
            
            return await Task.FromResult(datesToUse.AvailableDates);
        }
    }
}