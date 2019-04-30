using System;
using SFA.DAS.Reservations.Domain.Reservations;

namespace SFA.DAS.Reservations.Web.Models
{
    public class ReservationViewModel : AddApprenticeViewModel
    {
        public ReservationViewModel(
            Reservation reservation, 
            string apprenticeUrl, 
            string accountLegalEntityPublicHashedId) 
            : base(
                apprenticeUrl, 
                reservation.ProviderId, 
                reservation.Id, 
                accountLegalEntityPublicHashedId, 
                reservation.StartDate, 
                reservation.Course)
        {
            Id = reservation.Id;
            StartDateDescription = $"{reservation.StartDate:MMM yyyy} to {reservation.ExpiryDate:MMM yyyy}";
            Status = (ReservationStatusViewModel)reservation.Status;
            CourseName = reservation.Course != null ? reservation.Course.CourseDescription : "Unknown";
            LegalEntityName = reservation.AccountLegalEntityName;
        }

        public Guid Id { get; }
        public ReservationStatusViewModel Status { get; }
        public string LegalEntityName { get; }
        public string CourseName { get; }
        public string StartDateDescription { get; }
    }
}
