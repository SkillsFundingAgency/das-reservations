using System;
using System.Collections.Generic;
using SFA.DAS.Reservations.Domain.Courses;

namespace SFA.DAS.Reservations.Domain.Reservations.Api
{
    public class SearchReservationsApiResponse
    {
        public IEnumerable<SearchReservationApiModel> Reservations { get; set; }
        public int NumberOfRecordsFound { get; set; }
        public SearchFilters Filters { get; set; }
    }

    public class SearchReservationApiModel
    {
        public Guid Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public bool IsExpired { get; set; }
        public Course Course { get; set; }
        public string AccountLegalEntityName { get; set; }
        public long AccountLegalEntityId { get; set; }
        public uint? ProviderId { get; set; }
        public int Status { get; set; }
    }

    public class SearchFilters
    {
        public IEnumerable<string> EmployerFilters { get; set; }
        public IEnumerable<string> CourseFilters { get; set; }
        public IEnumerable<string> StartDateFilters { get; set; }
    }
}