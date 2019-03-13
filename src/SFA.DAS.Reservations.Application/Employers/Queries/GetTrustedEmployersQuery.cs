using MediatR;

namespace SFA.DAS.Reservations.Application.Employers.Queries
{
    public class GetTrustedEmployersQuery : IRequest<GetTrustedEmployersResponse>
    {
        public uint UkPrn { get; set; }
    }
}
