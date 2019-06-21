using System.Collections.Generic;

namespace SFA.DAS.Reservations.Web.Models
{
    public class SelectReservationViewModel
    {
        /*
           cohortReference
           transferSenderId
         */
        public IEnumerable<AvailableReservationViewModel> AvailableReservations { get; set; }
    }
}