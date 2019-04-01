using System;
using MediatR;

namespace SFA.DAS.Reservations.Application.Reservations.Commands
{
    public class CacheReservationStartDateCommand : IRequest<Unit>
    {
        public Guid Id { get; set; }
        public string StartDate { get; set; }
    }
}