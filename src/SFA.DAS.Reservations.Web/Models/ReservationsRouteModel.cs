using System;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Authorization.ModelBinding;

namespace SFA.DAS.Reservations.Web.Models
{
    public class ReservationsRouteModel 
    {
        [FromRoute]
        public uint? UkPrn { get; set; }
        public string EmployerAccountId { get; set; }
        public Guid? Id { get; set; }
        public string AccountLegalEntityPublicHashedId { get; set; }
        public bool? FromReview { get; set; }
        public bool? IsFromManage { get; set; }
        public string PreviousPage { get; set; }

        [FromQuery]
        public string CohortReference { get; set; }
        [FromQuery]
        public uint? ProviderId { get; set; }

        [FromQuery]
        public string JourneyData { get; set; }

    }
}