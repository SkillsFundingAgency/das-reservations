using System.Collections.Generic;

namespace SFA.DAS.Reservations.Domain.Reservations.Api
{
    public class AccountReservationStatusResponse
    {
        public bool CanAutoCreateReservations { get; set; }
        public Dictionary<long,bool> AccountLegalEntityAgreementStatus { get ; set ; }
    }
}
