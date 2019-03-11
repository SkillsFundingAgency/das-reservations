using System;
using MediatR;

namespace SFA.DAS.Reservations.Application.Reservations.Commands
{
    public class CacheCreateReservationCommand: ICreateReservationCommand, IRequest<CacheReservationResult>
    {
        public Guid? Id { get; set; }
        public string AccountId { get; set; }
        public string StartDate { get; set; }
    }
}