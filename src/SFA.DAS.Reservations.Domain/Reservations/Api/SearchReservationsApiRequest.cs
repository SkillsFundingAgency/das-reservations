using SFA.DAS.Reservations.Domain.Interfaces;

namespace SFA.DAS.Reservations.Domain.Reservations.Api
{
    public class SearchReservationsApiRequest : ISearchApiRequest
    {
        private readonly SearchReservationsRequest _request;

        public SearchReservationsApiRequest(string url, SearchReservationsRequest request)
        {
            _request = request;
            BaseUrl = url;
        }

        public string BaseUrl { get; }
        public string SearchUrl => $"{BaseUrl}api/reservations/search?providerId={_request.ProviderId}&searchTerm={_request.Filter.SearchTerm}&pageNumber={_request.Filter.PageNumber}&pageItemCount={_request.Filter.PageSize}";
    }
}