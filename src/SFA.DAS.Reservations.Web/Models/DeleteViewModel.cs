using System;
using System.ComponentModel.DataAnnotations;
using SFA.DAS.Reservations.Application.Reservations.Queries.GetReservation;
using SFA.DAS.Reservations.Domain.Courses;
using SFA.DAS.Reservations.Domain.Rules;
using SFA.DAS.Reservations.Web.Extensions;

namespace SFA.DAS.Reservations.Web.Models
{
    public class DeleteViewModel
    {
        public DeleteViewModel()
        {
            
        }

        public DeleteViewModel(GetReservationResult queryResult)
        {
            Id = queryResult.ReservationId;
            StartDateDescription = new TrainingDateModel
            {
                StartDate = queryResult.StartDate,
                EndDate = queryResult.ExpiryDate
            }.GetGDSDateString();
            AccountLegalEntityName = queryResult.AccountLegalEntityName;
            CourseDescription = queryResult.Course?.CourseDescription ?? 
                                new Course(null, null, 0).CourseDescription;
        }

        [Required(ErrorMessage = "Select whether you want to delete this reservation")]
        public bool? Delete { get; set; }
        public Guid Id { get; set; }
        public string StartDateDescription { get; set; }
        public string CourseDescription { get; set; }
        public string AccountLegalEntityName { get; set; }
    }
}