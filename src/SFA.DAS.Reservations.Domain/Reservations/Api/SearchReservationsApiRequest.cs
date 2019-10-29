using SFA.DAS.Reservations.Domain.Interfaces;

namespace SFA.DAS.Reservations.Domain.Reservations.Api
{
    public class SearchReservationsApiRequest : ISearchApiRequest
    {
        private readonly uint _providerId;
        private readonly SearchReservationsRequest _filter;

        public SearchReservationsApiRequest(string url, uint providerId, SearchReservationsRequest filter)
        {
            _providerId = providerId;
            _filter = filter;
            BaseUrl = url;
        }

        public string BaseUrl { get; }
        public string SearchUrl => $"{BaseUrl}api/reservations/search?providerId={_providerId}&searchTerm={_filter.SearchTerm}";
    }
}