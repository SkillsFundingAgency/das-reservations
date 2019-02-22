using System;

namespace SFA.DAS.Reservations.Domain.Interfaces
{
    public interface ICurrentDateTime
    {
        DateTime Now { get; }
    }
}