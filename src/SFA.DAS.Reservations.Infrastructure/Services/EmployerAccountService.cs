using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SFA.DAS.EAS.Account.Api.Client;
using SFA.DAS.Reservations.Domain.Authentication;
using SFA.DAS.Reservations.Domain.Employers;
using SFA.DAS.Reservations.Domain.Interfaces;

namespace SFA.DAS.Reservations.Infrastructure.Services
{
    public class EmployerAccountService : IEmployerAccountService
    {
        private readonly IAccountApiClient _accountApiClient;
        
        public EmployerAccountService(IAccountApiClient accountApiClient)
        {
            _accountApiClient = accountApiClient;
        }

        public async Task<IEnumerable<EmployerIdentifier>> GetEmployerIdentifiersAsync(string userId)
        {
            var accounts = await _accountApiClient.GetUserAccounts(userId);

            return accounts
                .Select(acc =>
                    new EmployerIdentifier { AccountId = acc.HashedAccountId, EmployerName = acc.DasAccountName });
        }

        private async Task<string> GetUserRole(EmployerIdentifier employerAccount, string userId)
        {
            var accounts = await _accountApiClient.GetAccountUsers(employerAccount.AccountId);

            if (accounts == null || !accounts.Any())
            {
                return null;
            }
            var teamMember = accounts.FirstOrDefault(c => String.Equals(c.UserRef, userId, StringComparison.CurrentCultureIgnoreCase));
            return teamMember?.Role;
        }

        public async Task<IEnumerable<EmployerIdentifier>> GetUserRoles(IEnumerable<EmployerIdentifier> values, string userId)
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

        public async Task<Claim> GetClaim(string userId, string claimType)
        {
            var accounts = await GetEmployerIdentifiersAsync(userId);

            accounts = await GetUserRoles(accounts, userId);

            var accountsAsJson = JsonConvert.SerializeObject(accounts.ToDictionary(k => k.AccountId));
            var associatedAccountsClaim = new Claim(claimType, accountsAsJson, JsonClaimValueTypes.Json);
            return associatedAccountsClaim;
        }

        public async Task<IEnumerable<EmployerTransferConnection>> GetTransferConnections(string accountId)
        {
            try
            {
                var transferConnections = await _accountApiClient.GetTransferConnections(accountId);
                return transferConnections.Select(acc => new EmployerTransferConnection
                {
                    FundingEmployerPublicHashedAccountId = acc.FundingEmployerPublicHashedAccountId,
                    FundingEmployerAccountName = acc.FundingEmployerAccountName,
                    FundingEmployerHashedAccountId = acc.FundingEmployerHashedAccountId,
                    FundingEmployerAccountId = acc.FundingEmployerAccountId
                });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            
            
        }
    }
}