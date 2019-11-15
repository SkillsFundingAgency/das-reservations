using System;
using System.Collections.Generic;
using SFA.DAS.Reservations.Domain.Reservations;

namespace SFA.DAS.Reservations.Web.Models
{
    public class ManageReservationsFilterModel
    {
        public const int PageSize = 50;
        public string SearchTerm { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PagedRecordsFrom => (PageNumber - 1) * PageSize + 1;
        public int PagedRecordsTo {
            get
            {
                var potentialValue = PageNumber * PageSize;
                return NumberOfRecordsFound < potentialValue ? NumberOfRecordsFound: potentialValue;
            }
        }
        public int NumberOfRecordsFound { get; set; }
        public IEnumerable<PageLink> PageLinks {
            get
            {
                var links = new List<PageLink>();
                var totalPages = (int)Math.Ceiling((double)NumberOfRecordsFound / PageSize);
                var totalPageLinks = totalPages < 5 ? totalPages : 5;

                //previous link
                if (totalPages > 1 && PageNumber > 1)
                {
                    links.Add(new PageLink
                    {
                        Label = "Previous",
                        AriaLabel = "Previous page",
                        RouteData = BuildRouteData(PageNumber-1)
                    });
                }

                //numbered links
                var pageNumberSeed = 1;
                if (totalPages > 5 && PageNumber > 3)
                {
                    pageNumberSeed = PageNumber - 2;

                    if (PageNumber > totalPages - 2)
                        pageNumberSeed = totalPages - 4;
                }

                for (var i = 0; i < totalPageLinks; i++)
                {
                    var link = new PageLink
                    {
                        Label = (pageNumberSeed + i).ToString(),
                        AriaLabel = $"Page {pageNumberSeed + i}",
                        IsCurrent = i+1==PageNumber? true : (bool?)null,
                        RouteData = BuildRouteData(pageNumberSeed + i)
                    };
                    links.Add(link);
                }

                //next link
                if (totalPages > 1 && PageNumber < totalPages)
                {
                    links.Add(new PageLink
                    {
                        Label = "Next",
                        AriaLabel = "Next page",
                        RouteData = BuildRouteData(PageNumber+1)
                    });
                }

                return links;
            }
        }

        public static implicit operator ReservationFilter(ManageReservationsFilterModel source)
        {
            return new ReservationFilter
            {
                SearchTerm = source.SearchTerm,
                PageNumber = source.PageNumber,
                PageSize = PageSize
            };
        }

        public IEnumerable<string> CourseFilters { get; set; }

        private Dictionary<string, string> BuildRouteData(int pageNumber)
        {
            var routeData = new Dictionary<string, string>();

            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                routeData.Add("searchTerm", SearchTerm);
            }

            routeData.Add("pageNumber", pageNumber.ToString());

            return routeData;
        }
    }

    public class PageLink
    {
        public string Label { get; set; }
        public string AriaLabel { get; set; }
        public bool? IsCurrent { get; set; }
        public Dictionary<string, string> RouteData { get; set; }
    }
}