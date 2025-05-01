using System;
using Microsoft.AspNetCore.Mvc;

namespace SFA.DAS.Reservations.Web.Models
{
    public class ReservationsRouteModel 
    {
        [FromRoute]
        public uint? UkPrn { get; set; }
        public string EmployerAccountId { get; set; }
        public string PublicHashedEmployerAccountId { get; set; }
        public Guid? Id { get; set; }
        public string AccountLegalEntityPublicHashedId { get; set; }
        public bool? FromReview { get; set; }
        public bool? IsFromManage { get; set; }
        public bool? IsFromSelect { get; set; }
        public string PreviousPage { get; set; }

        [FromQuery]
        public string CohortReference { get; set; }
        [FromQuery]
        public uint? ProviderId { get; set; }

        [FromQuery]
        public string JourneyData { get; set; }

        [FromQuery]
        public string SearchTerm { get; set; }

        [FromQuery]
        public string SortField { get; set; }

        [FromQuery]
        public bool ReverseSort { get; set; }

        [FromQuery]
        public bool? UseIlrData { get; set; }

    }
}