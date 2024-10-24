using System.Web;
using SFA.DAS.Reservations.Domain.Interfaces;

namespace SFA.DAS.Reservations.Domain.Employers.Api;

public class GetUserAccountsRequest: IGetApiRequest
{
    private readonly string _userId;
    private readonly string _email;

    public GetUserAccountsRequest(string baseUrl, string userId, string email)
    {
        BaseUrl = baseUrl;
        _userId = HttpUtility.UrlEncode(userId);
        _email = HttpUtility.UrlEncode(email);
    }

    public string GetUrl => $"{BaseUrl}/accountusers/{_userId}/accounts?email={_email}";
    public string BaseUrl { get; }
}