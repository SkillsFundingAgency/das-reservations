using MediatR;

namespace SFA.DAS.Reservations.Application.Providers.Queries.GetTrustedEmployers
{
    public class GetTrustedEmployersQuery : IRequest<GetTrustedEmployersResponse>
    {
        public uint UkPrn { get; set; }
    }
}
