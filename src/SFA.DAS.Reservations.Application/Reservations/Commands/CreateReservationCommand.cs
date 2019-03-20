using System;
using MediatR;

namespace SFA.DAS.Reservations.Application.Reservations.Commands
{
    public class CreateReservationCommand : IRequest<CreateReservationResult>
    {
        public Guid? Id { get; set; }
        public long AccountId { get; set; }
        public string StartDate { get; set; }
        public string CourseId { get; set; }
    }
}