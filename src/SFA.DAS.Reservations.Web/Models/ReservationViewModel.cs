using System;
using SFA.DAS.Reservations.Domain.Reservations;

namespace SFA.DAS.Reservations.Web.Models
{
    public class ReservationViewModel
    {
        public ReservationViewModel(Reservation reservation)
        {
            Id = reservation.Id;
            StartDateDescription = $"{reservation.StartDate:MMMM yyyy} to {reservation.ExpiryDate:MMMM yyyy}";
            Status = reservation.Status.ToString();
            CourseName = reservation.Course != null ? reservation.Course.CourseDescription : "Unknown";
            LegalEntityName = reservation.AccountLegalEntityName;
        }

        public Guid Id { get; }
        public string Status { get; }
        public string LegalEntityName { get; }
        public string CourseName { get; }
        public string StartDateDescription { get; }
    }
}
