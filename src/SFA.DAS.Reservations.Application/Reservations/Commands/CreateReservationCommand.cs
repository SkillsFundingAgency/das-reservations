using System;
using MediatR;

namespace SFA.DAS.Reservations.Application.Reservations.Commands
{
    public class CreateReservationCommand : IRequest<CreateReservationResult>
    {
        public Guid? Id { get; set; }
        public string HashedAccountId { get; set; }
        public string StartDate { get; set; }
        public string CourseId { get; set; }
    }
}