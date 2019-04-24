using MediatR;

namespace SFA.DAS.Reservations.Application.Reservations.Queries.GetReservations
{
    public class GetReservationsQuery : IRequest<GetReservationsResult>
    {
        public string AccountId { get; set; }
    }
}