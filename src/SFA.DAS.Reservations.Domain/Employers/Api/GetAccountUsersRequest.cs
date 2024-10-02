using SFA.DAS.Reservations.Domain.Interfaces;

namespace SFA.DAS.Reservations.Domain.Employers.Api;

public record GetAccountUsersRequest : IGetApiRequest
{
    private readonly long _accountId;

    public GetAccountUsersRequest(string baseUrl, long accountId)
    {
        _accountId = accountId;
        BaseUrl = baseUrl;
    }
    
    public string GetUrl => $"{BaseUrl}/accounts/{_accountId}/users";
    public string BaseUrl { get; }
}