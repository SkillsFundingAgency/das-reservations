using System;
using MediatR;

namespace SFA.DAS.Reservations.Application.Reservations.Commands
{
    public class CacheCreateReservationCommand: IRequest<CacheReservationResult>
    {
        public Guid? Id { get; set; }
        public string AccountId { get; set; }
        public string StartDate { get; set; }
        public string StartDateDescription { get; set; }
        public string CourseId { get; set; }
        public string CourseDescription { get; set; }
    }
}