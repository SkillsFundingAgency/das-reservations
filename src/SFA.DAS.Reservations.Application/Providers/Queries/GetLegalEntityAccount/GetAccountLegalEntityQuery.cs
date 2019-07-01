using MediatR;

namespace SFA.DAS.Reservations.Application.Providers.Queries.GetLegalEntityAccount
{
    public class GetAccountLegalEntityQuery : IRequest<GetAccountLegalEntityResult>
    {
        public string AccountLegalEntityPublicHashedId { get; set; }
    }
}
