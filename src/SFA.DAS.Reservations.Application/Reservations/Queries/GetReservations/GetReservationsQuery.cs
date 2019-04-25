using MediatR;

namespace SFA.DAS.Reservations.Application.Reservations.Queries.GetReservations
{
    public class GetReservationsQuery : IRequest<GetReservationsResult>
    {
        public long AccountId { get; set; }
    }
}