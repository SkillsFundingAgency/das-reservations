using MediatR;

namespace SFA.DAS.Reservations.Application.Reservations.Queries.SearchReservations
{
    public class SearchReservationsQuery : IRequest<SearchReservationsResult>
    {
        public uint ProviderId { get; set; }
        public string SearchTerm { get; set; }
    }
}