using SFA.DAS.Reservations.Domain.Interfaces;

namespace SFA.DAS.Reservations.Domain.Reservations.Api
{
    public class ReservationSearchApiRequest : ISearchApiRequest
    {
        private readonly uint _providerId;
        private readonly ReservationFilter _filter;

        public ReservationSearchApiRequest(string url, uint providerId, ReservationFilter filter)
        {
            _providerId = providerId;
            _filter = filter;
            BaseUrl = url;
        }

        public string BaseUrl { get; }
        public string SearchUrl => $"{BaseUrl}api/reservations/search?providerId={_providerId}&searchTerm={_filter.SearchTerm}";
    }
}