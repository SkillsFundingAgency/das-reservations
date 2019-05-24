using System;
using SFA.DAS.Reservations.Application.Reservations.Queries.GetReservation;
using SFA.DAS.Reservations.Domain.Courses;
using SFA.DAS.Reservations.Domain.Rules;

namespace SFA.DAS.Reservations.Web.Models
{
    public class DeleteViewModel
    {
        public DeleteViewModel(GetReservationResult queryResult)
        {
            ReservationId = queryResult.ReservationId;
            StartDateDescription = new StartDateModel
            {
                StartDate = queryResult.StartDate,
                EndDate = queryResult.ExpiryDate
            }.ToString();
            AccountLegalEntityName = queryResult.AccountLegalEntityName;
            CourseDescription = queryResult.Course?.CourseDescription ?? 
                                new Course(null, null, 0).CourseDescription;
        }

        public Guid ReservationId { get; }
        public string StartDateDescription { get; }
        public string CourseDescription { get; }
        public string AccountLegalEntityName { get; }
    }
}