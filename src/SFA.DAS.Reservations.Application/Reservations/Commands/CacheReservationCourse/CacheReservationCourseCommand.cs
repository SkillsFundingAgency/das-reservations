using System;
using MediatR;

namespace SFA.DAS.Reservations.Application.Reservations.Commands.CacheReservationCourse
{
    public class CacheReservationCourseCommand: IRequest<CacheReservationResult>
    {
        public Guid? Id { get; set; }
       
        public string CourseId { get; set; }
    }
}