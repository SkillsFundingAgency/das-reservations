using System;
using SFA.DAS.Reservations.Application.Reservations.Commands;

namespace SFA.DAS.Reservations.Application.Reservations.Queries
{
    public class GetCachedReservationResult : ICreateReservationCommand
    {
        public Guid? Id { get; set; }
        public string StartDate { get; set; }
        public string AccountId { get; set; }
    }
}