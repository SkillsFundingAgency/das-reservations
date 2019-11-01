using MediatR;
using SFA.DAS.Reservations.Domain.Reservations;

namespace SFA.DAS.Reservations.Application.Reservations.Queries.SearchReservations
{
    public class SearchReservationsQuery : IRequest<SearchReservationsResult>
    {
        public uint ProviderId { get; set; }
        public ReservationFilter Filter { get; set; }

        public static implicit operator SearchReservationsRequest(SearchReservationsQuery source)
        {
            return new SearchReservationsRequest
            {
                ProviderId = source.ProviderId,
                Filter = source.Filter
            };
        }
    }
}