using System;

namespace SFA.DAS.Reservations.Web.Models
{
    public class ReservationsRouteModel
    {
        public uint? UkPrn { get; set; }
        public string EmployerAccountId { get; set; }
        public Guid? Id { get; set; }
        public string AccountLegalEntityPublicHashedId { get; set; }
        public bool? FromReview { get; set; }
    }
}