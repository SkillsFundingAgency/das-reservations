﻿using System.Collections.Generic;
using SFA.DAS.Reservations.Domain.Reservations;

namespace SFA.DAS.Reservations.Application.Reservations.Queries.SearchReservations
{
    public class SearchReservationsResult
    {
        public IEnumerable<Reservation> Reservations { get; set; }
        public int NumberOfRecordsFound { get; set; }
        public IEnumerable<string> EmployerFilters { get; set; }
        public IEnumerable<string> CourseFilters { get; set; }
        public IEnumerable<string> StartDateFilters { get; set; }

        public int TotalReservationsForProvider { get; set; }

        public static implicit operator SearchReservationsResult(SearchReservationsResponse source)
        {
            return new SearchReservationsResult
            {
                Reservations = source.Reservations,
                NumberOfRecordsFound = source.NumberOfRecordsFound,
                EmployerFilters = source.EmployerFilters,
                CourseFilters = source.CourseFilters,
                StartDateFilters = source.StartDateFilters,
                TotalReservationsForProvider = source.TotalReservationsForProvider
            };
        }
    }
}