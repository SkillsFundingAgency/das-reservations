using System;

namespace SFA.DAS.Reservations.Application.Reservations.Commands
{
    public interface ICreateReservationCommand
    {
        Guid? Id { get; set; }
        string AccountId { get; set; }
        string StartDate { get; set; }
    }
}