using System.Collections.Generic;
using SFA.DAS.Reservations.Infrastructure.Configuration;

namespace SFA.DAS.Reservations.Web.Models
{
    public class ManageViewModel
    {
        public List<ReservationViewModel> Reservations { get; set; }
        public string BackLink { get; set; }
        public ManageReservationsFilterModel FilterModel { get; set; }
        public bool ShowSearch => TotalReservationCount >= ReservationsWebConfigurationConstants.NumberOfReservationsRequiredForSearch;
        public int TotalReservationCount { get; set; }
        public bool ShowFilteredSearch =>
            FilterModel.NumberOfRecordsFound > ReservationsWebConfigurationConstants.NumberOfReservationsPerSearchPage;
    }
}