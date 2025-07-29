using SFA.DAS.Reservations.Application.Reservations.Commands.CacheReservationEmployer;
using SFA.DAS.Reservations.Domain.Employers;

namespace SFA.DAS.Reservations.Application.Reservations.Queries.GetProviderCacheReservationCommand
{
    public class GetProviderCacheReservationCommandResponse
    {
        public AccountLegalEntity LegalEntity { get; set; }
        public CacheReservationEmployerCommand Command { get; set; }
    }
}
