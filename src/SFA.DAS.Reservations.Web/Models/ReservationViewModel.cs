using System;
using SFA.DAS.Reservations.Domain.Reservations;
using SFA.DAS.Reservations.Domain.Rules;

namespace SFA.DAS.Reservations.Web.Models
{
    public class ReservationViewModel : AddApprenticeViewModel
    {
        public ReservationViewModel(Reservation reservation, string apprenticeUrl, bool canBeDeleted) : base(apprenticeUrl)
        {
            Id = reservation.Id;
            TrainingDate = new TrainingDateModel
            {
                StartDate = reservation.StartDate, 
                EndDate = reservation.ExpiryDate
            };
            
            Status = (ReservationStatusViewModel)reservation.Status;
            CourseName = reservation.Course != null ? reservation.Course.CourseDescription : "Unknown";
            LegalEntityName = reservation.AccountLegalEntityName;
            CanBeDeleted = canBeDeleted;
        }

        public Guid Id { get; }
        public ReservationStatusViewModel Status { get; }
        public string LegalEntityName { get; }
        public string CourseName { get; }
        public TrainingDateModel TrainingDate { get; }
        public bool CanBeDeleted { get; set; }
    }
}
