using MediatR;

namespace SFA.DAS.Reservations.Application.Employers.Queries.GetLegalEntity
{
    public class GetLegalEntityQuery : IRequest<GetLegalEntityResponse>
    {
        public long Id { get; set; }
    }
}   