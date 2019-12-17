using System.Collections.Generic;
using SFA.DAS.Reservations.Domain.Employers;

namespace SFA.DAS.Reservations.Application.Employers.Queries.GetAccountUsers
{
    public class GetAccountUsersResponse
    {
        public IEnumerable<EmployerAccountUser> AccountUsers { get; set; }
    }
}