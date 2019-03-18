using System;

namespace SFA.DAS.Reservations.Web.Models
{
    public class ReservationsRouteModel
    {
        public long? Ukprn { get; set; }
        public string EmployerAccountId { get; set; }
        public Guid? Id { get; set; }
    }
}