﻿using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Html;
using SFA.DAS.Reservations.Domain.Reservations;
using SFA.DAS.Reservations.Infrastructure.Configuration;

namespace SFA.DAS.Reservations.Web.Models
{
    public class ManageReservationsFilterModelBase
    {

        public int PageNumber { get; set; } = 1;
        public string SearchTerm { get; set; }
        public string SelectedEmployer { get; set; }
        public string SelectedCourse { get; set; }
        public string SelectedStartDate { get; set; }


    }

    public class ManageReservationsFilterModel : ManageReservationsFilterModelBase
    {
		public IEnumerable<string> EmployerFilters { get; set; } = new List<string>();
        public IEnumerable<string> CourseFilters { get; set; } = new List<string>();
        public IEnumerable<string> StartDateFilters { get; set; } = new List<string>();
        public const int PageSize = ReservationsWebConfigurationConstants.NumberOfReservationsPerSearchPage;
        public int PagedRecordsFrom => NumberOfRecordsFound == 0 ? 0 : (PageNumber - 1) * PageSize + 1;
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
                        IsCurrent = pageNumberSeed + i == PageNumber? true : (bool?)null,
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

        public bool SearchOrFiltersApplied => !string.IsNullOrWhiteSpace(SearchTerm)
                                     || !string.IsNullOrWhiteSpace(SelectedEmployer)
                                     || !string.IsNullOrWhiteSpace(SelectedCourse)
                                     || !string.IsNullOrWhiteSpace(SelectedStartDate);

        public HtmlString FiltersUsedMessage
        {
            get
            {
                var filters = new List<string>();
                if (!string.IsNullOrWhiteSpace(SearchTerm)) filters.Add($"‘{SearchTerm}’");
                if (!string.IsNullOrWhiteSpace(SelectedEmployer)) filters.Add(SelectedEmployer);
                if (!string.IsNullOrWhiteSpace(SelectedCourse)) filters.Add(SelectedCourse);
                if (!string.IsNullOrWhiteSpace(SelectedStartDate)) filters.Add(SelectedStartDate);

                if (filters.Count == 0) return HtmlString.Empty;

                var message = new StringBuilder();

                message.Append($"matching <strong>{filters[0]}</strong>");

                for (var i = 1; i < filters.Count; i++)
                {
                    if (i == filters.Count-1)
                    {
                        message.Append(" and ");
                    }
                    else
                    {
                        message.Append(", ");
                    }

                    message.Append($"<strong>{filters[i]}</strong>");
                }

                return new HtmlString(message.ToString());
            }
        }

        public static implicit operator ReservationFilter(ManageReservationsFilterModel source)
        {
            return new ReservationFilter
            {
                SearchTerm = source.SearchTerm,
                PageNumber = source.PageNumber,
                PageSize = PageSize,
                SelectedEmployer = source.SelectedEmployer,
                SelectedCourse = source.SelectedCourse,
                SelectedStartDate = source.SelectedStartDate
            };
        }

        private Dictionary<string, string> BuildRouteData(int pageNumber)
        {
            var routeData = new Dictionary<string, string>();

            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                routeData.Add("searchTerm", SearchTerm);
            }

            if (!string.IsNullOrWhiteSpace(SelectedCourse))
            {
                routeData.Add("selectedCourse", SelectedCourse);
            }

            if (!string.IsNullOrWhiteSpace(SelectedEmployer))
            {
                routeData.Add("selectedEmployer", SelectedEmployer);
            }
            
            if (!string.IsNullOrWhiteSpace(SelectedStartDate))
            {
                routeData.Add("selectedStartDate", SelectedStartDate);
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