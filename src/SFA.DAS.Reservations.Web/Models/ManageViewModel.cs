using System.Collections.Generic;

namespace SFA.DAS.Reservations.Web.Models
{
    public class ManageViewModel
    {
        public List<ReservationViewModel> Reservations { get; set; }
        public string BackLink { get; set; }
        public ManageReservationsFilterModel FilterModel { get; set; }
        public int NumberOfRecordsFound { get; set; }
    }
}