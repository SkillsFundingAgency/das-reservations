using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SFA.DAS.EAS.Account.Api.Client;
using SFA.DAS.Reservations.Domain.Employers;
using SFA.DAS.Reservations.Domain.Employers.Api;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Infrastructure.Api;
using SFA.DAS.Reservations.Infrastructure.Configuration;
using EmployerIdentifier = SFA.DAS.Reservations.Domain.Authentication.EmployerIdentifier;

namespace SFA.DAS.Reservations.Infrastructure.Services
{
    public class EmployerAccountService : IEmployerAccountService
    {
        private readonly IAccountApiClient _accountApiClient;
        private readonly IReservationsOuterApiClient _reservationsOuterApiClient;
        private readonly ReservationsOuterApiConfiguration _outerApiConfiguration;
        private readonly ReservationsWebConfiguration _configuration;

        public EmployerAccountService(IAccountApiClient accountApiClient, IOptions<ReservationsWebConfiguration> configuration, IReservationsOuterApiClient reservationsOuterApiClient,IOptions<ReservationsOuterApiConfiguration> outerApiConfiguration)
        {
            _accountApiClient = accountApiClient;
            _reservationsOuterApiClient = reservationsOuterApiClient;
            _outerApiConfiguration = outerApiConfiguration.Value;
            _configuration = configuration.Value;
        }

        public async Task<List<Claim>> GetClaim(string userId, string claimType, string email)
        {
            IEnumerable<EmployerIdentifier> accounts;
            var claims = new List<Claim>();
            if (_configuration.UseGovSignIn)
            {
                var apiResponse =
                    await _reservationsOuterApiClient.Get<GetUserAccountsResponse>(new GetUserAccountsRequest(_outerApiConfiguration.ApiBaseUrl, userId, email));
                if (apiResponse.IsSuspended)
                {
                    claims.Add(new Claim(ClaimTypes.AuthorizationDecision, "Suspended"));
                }
                
                claims.Add(new Claim(EmployerClaims.IdamsUserIdClaimTypeIdentifier, apiResponse.UserId));
                
                accounts = apiResponse.UserAccounts.Select(c => new EmployerIdentifier
                {
                    Role = c.Role,
                    AccountId = c.AccountId,
                    EmployerName = c.EmployerName
                });
            }
            else
            {
                accounts = await GetEmployerIdentifiersAsync(userId);
                accounts = await GetUserRoles(accounts, userId);    
            }
            

            var accountsAsJson = JsonConvert.SerializeObject(accounts.ToDictionary(k => k.AccountId));
            var associatedAccountsClaim = new Claim(claimType, accountsAsJson, JsonClaimValueTypes.Json);
            claims.Add(associatedAccountsClaim);
            return claims;
        }

        public async Task<IEnumerable<EmployerIdentifier>> GetEmployerIdentifiersAsync(string userId)
        {
            var accounts = await _accountApiClient.GetUserAccounts(userId);

            return accounts
                .Select(acc =>
                    new EmployerIdentifier { AccountId = acc.HashedAccountId, EmployerName = acc.DasAccountName });
        }


        public async Task<IEnumerable<EmployerAccountUser>> GetAccountUsers(long accountId)
        {
            var teamMembers = await _accountApiClient.GetAccountUsers(accountId);

            var users = teamMembers.Select(model => (EmployerAccountUser)model);

            return users;
        }

        private async Task<IEnumerable<EmployerIdentifier>> GetUserRoles(IEnumerable<EmployerIdentifier> values, string userId)
        {
            var employerIdentifiers = values.ToList();

            var identifiersToRemove = new List<EmployerIdentifier>();

            foreach (var employerIdentifier in employerIdentifiers)
            {
                var result = await GetUserRole(employerIdentifier, userId);

                if (result != null)
                {
                    employerIdentifier.Role = result;
                }
                else
                {
                    identifiersToRemove.Add(employerIdentifier);
                }
            }

            return employerIdentifiers.Except(identifiersToRemove);
        }

        private async Task<string> GetUserRole(EmployerIdentifier employerAccount, string userId)
        {
            var accounts = await _accountApiClient.GetAccountUsers(employerAccount.AccountId);

            if (accounts == null || !accounts.Any())
            {
                return null;
            }
            var teamMember = accounts.FirstOrDefault(c => string.Equals(c.UserRef, userId, StringComparison.CurrentCultureIgnoreCase));
            return teamMember?.Role;
        }
    }
}