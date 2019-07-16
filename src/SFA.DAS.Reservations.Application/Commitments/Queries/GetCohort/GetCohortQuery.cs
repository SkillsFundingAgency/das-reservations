using MediatR;

namespace SFA.DAS.Reservations.Application.Commitments.Queries.GetCohort
{
    public class GetCohortQuery : IRequest<GetCohortResponse>
    {
        public long CohortId { get; set; }
    }
}
