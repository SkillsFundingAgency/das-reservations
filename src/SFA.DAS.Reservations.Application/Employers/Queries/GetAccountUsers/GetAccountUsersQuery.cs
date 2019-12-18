using MediatR;

namespace SFA.DAS.Reservations.Application.Employers.Queries.GetAccountUsers
{
    public class GetAccountUsersQuery : IRequest<GetAccountUsersResponse>
    {
        public long AccountId { get; set; }
    }
}