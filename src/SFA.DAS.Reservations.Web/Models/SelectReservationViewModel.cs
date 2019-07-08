using System;
using System.Collections.Generic;

namespace SFA.DAS.Reservations.Web.Models
{
    public class SelectReservationViewModel
    {
        public Guid SelectedReservationId { get; set; }

        public string CohortReference { get; set; }

        public string TransferSenderId { get; set; }
        public IEnumerable<AvailableReservationViewModel> AvailableReservations { get; set; }
        public long AccountId { get; set; }
    }
}