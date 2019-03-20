using System;

namespace SFA.DAS.Reservations.Application.Reservations.Commands
{
    public interface ICreateReservationCommand
    {
        string AccountId { get; set; }
        string StartDate { get; set; }
        string CourseId { get; set; }
    }
}