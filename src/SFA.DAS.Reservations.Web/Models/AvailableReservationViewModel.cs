using System;
using SFA.DAS.Reservations.Domain.Reservations;
using SFA.DAS.Reservations.Domain.Rules;
using SFA.DAS.Reservations.Web.Extensions;

namespace SFA.DAS.Reservations.Web.Models
{
    public class AvailableReservationViewModel
    {
        public AvailableReservationViewModel()
        {
            
        }
        
        public AvailableReservationViewModel(Reservation reservation)
        {
            ReservationId = reservation.Id;
            CourseDescription = reservation.Course.CourseDescription;
            TrainingDateDescription = new TrainingDateModel
            {
                StartDate = reservation.StartDate,
                EndDate = reservation.ExpiryDate
            }.GetGDSDateString();
            CreatedDateDescription = reservation.CreatedDate.GetGDSLongDateString();
        }

        public Guid ReservationId { get; set; }
        public string CourseDescription { get; set; }
        public string TrainingDateDescription { get; set; }
        public string CreatedDateDescription { get; set; }
    }
}