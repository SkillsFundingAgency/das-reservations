﻿using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using SFA.DAS.Reservations.Domain.Authentication;
using SFA.DAS.Reservations.Domain.Employers;

namespace SFA.DAS.Reservations.Domain.Interfaces
{
    public interface IEmployerAccountService
    {
        Task<IEnumerable<EmployerIdentifier>> GetEmployerIdentifiersAsync(string userId);
        Task<IEnumerable<EmployerIdentifier>> GetUserRoles(IEnumerable<EmployerIdentifier> values, string userId);
        Task<Claim> GetClaim(string userId, string claimType);
        Task<IEnumerable<EmployerTransferConnection>> GetTransferConnections(string accountId);
        Task<IEnumerable<EmployerAccountUser>> GetUsersForAccount(long accountId);
    }
}