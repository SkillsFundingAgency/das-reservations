using System;
using SFA.DAS.Common.Domain.Types;
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
            UkPrn = queryResult.UkPrn;
            StartDateDescription = new TrainingDateModel
            {
                StartDate = queryResult.StartDate,
                EndDate = queryResult.ExpiryDate
            }.GetGDSDateString();
            AccountLegalEntityName = queryResult.AccountLegalEntityName;
            CourseDescription = queryResult.Course?.CourseDescription ??
                                new Course(null, null, 0).CourseDescription;
            LearningType = queryResult.Course?.LearningType;
        }

        public uint? UkPrn { get; set; }

        public bool? Delete { get; set; }
        public Guid Id { get; set; }
        public string StartDateDescription { get; set; }
        public string CourseDescription { get; set; }
        public string AccountLegalEntityName { get; set; }
        public LearningType? LearningType { get; set; }
    }
}