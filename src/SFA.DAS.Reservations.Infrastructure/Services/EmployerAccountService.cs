using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SFA.DAS.Reservations.Domain.Employers;
using SFA.DAS.Reservations.Domain.Employers.Api;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Infrastructure.Api;
using SFA.DAS.Reservations.Infrastructure.Configuration;

namespace SFA.DAS.Reservations.Infrastructure.Services;

public class EmployerAccountService : IEmployerAccountService
{
    private readonly IReservationsOuterApiClient _reservationsOuterApiClient;
    private readonly ReservationsOuterApiConfiguration _outerApiConfiguration;

    public EmployerAccountService(IReservationsOuterApiClient reservationsOuterApiClient, IOptions<ReservationsOuterApiConfiguration> outerApiConfiguration)
    {
        _reservationsOuterApiClient = reservationsOuterApiClient;
        _outerApiConfiguration = outerApiConfiguration.Value;
    }

    public async Task<List<Claim>> GetClaim(string userId, string claimType, string email)
    {
        var claims = new List<Claim>();

        var accountsRequest = new GetUserAccountsRequest(_outerApiConfiguration.ApiBaseUrl, userId, email);
        var apiResponse = await _reservationsOuterApiClient.Get<GetUserAccountsResponse>(accountsRequest);

        if (apiResponse.IsSuspended)
        {
            claims.Add(new Claim(ClaimTypes.AuthorizationDecision, "Suspended"));
        }

        claims.Add(new Claim(EmployerClaims.IdamsUserIdClaimTypeIdentifier, apiResponse.UserId));

        var accounts = apiResponse.UserAccounts.Select(c => new EmployerIdentifier
        {
            Role = c.Role,
            AccountId = c.AccountId,
            EmployerName = c.EmployerName
        });

        var accountsAsJson = JsonConvert.SerializeObject(accounts.ToDictionary(k => k.AccountId));
        var associatedAccountsClaim = new Claim(claimType, accountsAsJson, JsonClaimValueTypes.Json);

        claims.Add(associatedAccountsClaim);

        return claims;
    }

    public async Task<IEnumerable<EmployerAccountUser>> GetAccountUsers(long accountId)
    {
        var request = new GetAccountUsersRequest(_outerApiConfiguration.ApiBaseUrl, accountId);
        var response = await _reservationsOuterApiClient.Get<GetAccountUsersApiResponse>(request);
        
        return response.AccountUsers.Select(model => (EmployerAccountUser)model);
    }
}