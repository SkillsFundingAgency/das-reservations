using MediatR;

namespace SFA.DAS.Reservations.Application.Reservations.Queries.GetProviderCacheReservationCommand
{
    public class GetProviderCacheReservationCommandQuery : IRequest<GetProviderCacheReservationCommandResponse>
    {
        public uint UkPrn { get; set; }
        public string AccountLegalEntityPublicHashedId { get; set; }
        public string CohortRef { get; set; }
        public long? CohortId { get; set; }
    }
}
