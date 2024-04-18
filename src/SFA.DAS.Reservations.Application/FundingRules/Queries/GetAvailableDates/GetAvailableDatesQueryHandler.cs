using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.Reservations.Domain.Interfaces;

namespace SFA.DAS.Reservations.Application.FundingRules.Queries.GetAvailableDates
{
    public class GetAvailableDatesQueryHandler : IRequestHandler<GetAvailableDatesQuery, GetAvailableDatesResult>
    {
        private readonly IReservationsOuterService _reservationOuterService;

        public GetAvailableDatesQueryHandler(IReservationsOuterService reservationOuterService)
        {
            _reservationOuterService = reservationOuterService;
        }

        public async Task<GetAvailableDatesResult> Handle(GetAvailableDatesQuery request, CancellationToken cancellationToken)
        {
            var availableDates = await _reservationOuterService.GetAvailableDates(request.AccountLegalEntityId);

            return new GetAvailableDatesResult
            {
                AvailableDates = availableDates.AvailableDates
            };
        }
    }
}
