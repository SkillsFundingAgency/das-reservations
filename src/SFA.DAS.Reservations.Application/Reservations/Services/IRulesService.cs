using System.Collections.Generic;
using SFA.DAS.Reservations.Domain.Rules;

namespace SFA.DAS.Reservations.Application.Reservations.Services
{
    public interface IRulesService
    {
        IEnumerable<ReservationRule> GetReservationRules();
    }
}
