using System.Text;
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
        public string SearchUrl
        {
            get
            {
                var url = new StringBuilder();

                url.Append($"{BaseUrl}api/reservations/search");
                url.Append($"?providerId={_request.ProviderId}");
                url.Append($"&pageNumber={_request.Filter.PageNumber}");
                url.Append($"&pageItemCount={_request.Filter.PageSize}");
                if(!string.IsNullOrWhiteSpace(_request.Filter.SearchTerm)) 
                    url.Append($"&searchTerm={_request.Filter.SearchTerm}");
                if(!string.IsNullOrWhiteSpace(_request.Filter.SelectedEmployer)) 
                    url.Append($"&selectedEmployer={_request.Filter.SelectedEmployer}");
                if(!string.IsNullOrWhiteSpace(_request.Filter.SelectedCourse)) 
                    url.Append($"&selectedCourse={_request.Filter.SelectedCourse}");
                if(!string.IsNullOrWhiteSpace(_request.Filter.SelectedStartDate)) 
                    url.Append($"&selectedStartDate={_request.Filter.SelectedStartDate}");

                return url.ToString();
            }
        }
    }
}