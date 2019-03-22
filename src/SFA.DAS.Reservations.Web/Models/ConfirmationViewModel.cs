using System;
using SFA.DAS.Reservations.Domain.Courses;

namespace SFA.DAS.Reservations.Web.Models
{
    public class ConfirmationViewModel
    {
        public ConfirmationViewModel(Guid reservationId, DateTime startDate, DateTime expiryDate, Course course)
        {
            ReservationId = reservationId;
            StartDate = startDate;
            ExpiryDate = expiryDate;
            Course = course;
        }

        public Guid ReservationId { get; }
        public DateTime StartDate { get; }
        public DateTime ExpiryDate { get; }
        public Course Course { get; }
    }
}