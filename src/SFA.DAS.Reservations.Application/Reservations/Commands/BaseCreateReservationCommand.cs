using System;

namespace SFA.DAS.Reservations.Application.Reservations.Commands
{
    public class BaseCreateReservationCommand
    {
        public Guid? Id { get; set; }
        public string AccountId { get; set; }
        public string StartDate { get; set; }
    }
}