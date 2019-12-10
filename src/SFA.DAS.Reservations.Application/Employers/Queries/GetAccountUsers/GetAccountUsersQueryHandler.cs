using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.Reservations.Domain.Interfaces;

namespace SFA.DAS.Reservations.Application.Employers.Queries.GetAccountUsers
{
    public class GetAccountUsersQueryHandler : IRequestHandler<GetAccountUsersQuery, GetAccountUsersResponse>
    {
        private readonly IEmployerAccountService _employerAccountService;

        public GetAccountUsersQueryHandler(IEmployerAccountService employerAccountService)
        {
            _employerAccountService = employerAccountService;
        }

        public async Task<GetAccountUsersResponse> Handle(GetAccountUsersQuery request, CancellationToken cancellationToken)
        {
            var users = await _employerAccountService.GetAccountUsers(request.AccountId);

            return new GetAccountUsersResponse
            {
                AccountUsers = users
            };
        }
    }
}