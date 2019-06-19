using MediatR;

namespace SFA.DAS.Reservations.Application.Reservations.Queries.GetAvailableReservations
{
    public class GetAvailableReservationsQuery : IRequest<GetAvailableReservationsResult>
    {
        public long AccountId { get; set; }
    }
}