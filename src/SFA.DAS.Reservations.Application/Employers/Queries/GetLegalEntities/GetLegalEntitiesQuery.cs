using MediatR;

namespace SFA.DAS.Reservations.Application.Employers.Queries.GetLegalEntities
{
    public class GetLegalEntitiesQuery : IRequest<GetLegalEntitiesResponse>
    {
        public string AccountId { get; set; }
    }
}