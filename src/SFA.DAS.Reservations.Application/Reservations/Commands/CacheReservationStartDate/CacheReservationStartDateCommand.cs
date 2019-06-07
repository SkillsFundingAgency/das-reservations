using System;
using MediatR;
using SFA.DAS.Reservations.Domain.Rules;

namespace SFA.DAS.Reservations.Application.Reservations.Commands.CacheReservationStartDate
{
    public class CacheReservationStartDateCommand : IRequest<Unit>
    {
        public Guid Id { get; set; }
        public StartDateModel TrainingDate { get; set; }
        public uint UkPrn { get; set; }
    }
}