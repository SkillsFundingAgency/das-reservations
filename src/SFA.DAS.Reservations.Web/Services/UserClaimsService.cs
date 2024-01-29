using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SFA.DAS.Reservations.Domain.Authentication;
using SFA.DAS.Reservations.Domain.Employers;
using SFA.DAS.Reservations.Infrastructure.Services;

namespace SFA.DAS.Reservations.Web.Services
{
    public class UserClaimsService : IUserClaimsService
    {
        private readonly ILogger<UserClaimsService> _logger;

        public UserClaimsService(ILogger<UserClaimsService> logger)
        {
            _logger = logger;
        }

        public bool UserIsInRole(string employerAccountId, EmployerUserRole userRole, IEnumerable<Claim> claims)
        {
            try
            {
                var accountsClaim = claims.First(c => c.Type == EmployerClaims.AccountsClaimsTypeIdentifier);
                var employerAccounts = JsonConvert.DeserializeObject<Dictionary<string, EmployerIdentifier>>(accountsClaim.Value);
                var employerIdentifier = employerAccounts[employerAccountId];

                return employerIdentifier.Role == userRole.ToString();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error while trying to assert employer user role.");
                return false;
            }
        }

        public List<EmployerIdentifier> GetAllAssociatedAccounts(IEnumerable<Claim> claims)
        {
            var accountsClaims = claims
               .Where(c => c.Type.Equals(EmployerClaims.AccountsClaimsTypeIdentifier))
               .Select(c => c.Value)
               .ToList();

            if (accountsClaims.Any())
            {
                var accounts = JsonConvert.DeserializeObject<Dictionary<string, EmployerIdentifier>>(
                    accountsClaims.FirstOrDefault())
                    .Select(c => c.Value)
                    .ToList();

                return accounts;
            }

            return new List<EmployerIdentifier>();
        }
    }
}