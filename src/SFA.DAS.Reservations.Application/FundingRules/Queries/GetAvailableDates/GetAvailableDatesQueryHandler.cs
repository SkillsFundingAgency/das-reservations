using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.Reservations.Domain.Interfaces;

namespace SFA.DAS.Reservations.Application.FundingRules.Queries.GetAvailableDates;

public class GetAvailableDatesQueryHandler(IReservationsOuterService reservationOuterService)
    : IRequestHandler<GetAvailableDatesQuery, GetAvailableDatesResult>
{
    public async Task<GetAvailableDatesResult> Handle(GetAvailableDatesQuery request, CancellationToken cancellationToken)
    {
        var availableDates = await reservationOuterService.GetAvailableDates(request.AccountLegalEntityId, request.CourseId);

        return new GetAvailableDatesResult
        {
            AvailableDates = availableDates.AvailableDates
        };
    }
}