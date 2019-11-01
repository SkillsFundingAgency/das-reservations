using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Reservations.Domain.Courses;
using SFA.DAS.Reservations.Domain.Employers;
using SFA.DAS.Reservations.Domain.Reservations.Api;
using SFA.DAS.Reservations.Domain.Rules;
using SFA.DAS.Reservations.Web.Models;

namespace SFA.DAS.Reservations.Web.AcceptanceTests.Infrastructure
{
    public class TestData
    {
        public ReservationsRouteModel ReservationRouteModel { get; set; }
        public AccountLegalEntity AccountLegalEntity { get; set; }
        public Course Course { get; set; }
        public TrainingDateModel TrainingDate { get; set; }
        public Guid UserId { get; set; }

        public List<GetReservationResponse> Reservations { get; set; }
        public IActionResult ActionResult { get; set; }

        public void BuildTrainingDateModel(string startMonth)
        {
            var startDate = DateTime.Parse($"{DateTime.UtcNow.Year} {startMonth} 01");
            
            TrainingDate = new TrainingDateModel
            {
                StartDate = startDate,
                EndDate = startDate.AddMonths(2)
            };
        }
    }
}