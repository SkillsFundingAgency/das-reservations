using System;
using System.Collections.Generic;

namespace SFA.DAS.Reservations.Web.Models
{
    public class SortModel
    {
        public string SortField { get; set; }
        public bool ReverseSort { get; set; }

        public Dictionary<string, string> BuildSortRouteData(string sortField, string searchTerm)
        {
            var routeData = new Dictionary<string, string>();

            var reverseSort = !string.IsNullOrEmpty(SortField)
                              && string.Equals(SortField, sortField, StringComparison.CurrentCultureIgnoreCase)
                              && !ReverseSort;

            routeData.Add(nameof(ReverseSort), reverseSort.ToString());
            routeData.Add(nameof(SortField), sortField);
            routeData.Add(nameof(searchTerm), searchTerm);

            return routeData;
        }
    }
}
