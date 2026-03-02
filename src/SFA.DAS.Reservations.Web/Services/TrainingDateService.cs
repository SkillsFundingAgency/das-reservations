using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.Reservations.Application.FundingRules.Queries.GetAvailableDates;
using SFA.DAS.Reservations.Domain.Rules;

namespace SFA.DAS.Reservations.Web.Services;

public class TrainingDateService(IMediator mediator) : ITrainingDateService
{
    public async Task<IEnumerable<TrainingDateModel>> GetTrainingDates(long accountLegalEntityId, string courseId = null)
    {
        var datesToUse = await mediator.Send(new GetAvailableDatesQuery
        {
            AccountLegalEntityId = accountLegalEntityId,
            CourseId = courseId
        });

        return await Task.FromResult(datesToUse.AvailableDates);
    }
}