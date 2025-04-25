using System;
using System.Collections.Generic;

namespace SFA.DAS.Reservations.Web.Models
{
    public class SelectReservationViewModel
    {
        public string CohortReference { get; set; }
        public string TransferSenderId { get; set; }
        public string EncodedPledgeApplicationId { get; set; }
        public string JourneyData { get; set; }
        public Guid? SelectedReservationId { get; set; }
        public IEnumerable<AvailableReservationViewModel> AvailableReservations { get; set; }
        public long AccountId { get; set; }
        public string BackLink { get; set; }
        public uint? ProviderId { get; set; }
        public bool MoreReservationsAvailable { get; set; }
        public Guid? ApprenticeshipSessionKey { get; set; }
        public bool? BeforeProviderSelected { get; set; }
        public bool? IlrMode { get; set; }
    }
}