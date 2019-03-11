using System;

namespace SFA.DAS.Reservations.Application.Reservations.Commands
{
    public interface ICreateReservationCommand
    {
        Guid? Id { get; }
        string AccountId { get; }
        string StartDate { get; }
    }
}