using System.Collections.Generic;

namespace SFA.DAS.Reservations.Domain.Employers.Api;

public record GetAccountUsersApiResponse
{
    public List<AccountUsersResponseItem> AccountUsers { get; set; }
}

public class AccountUsersResponseItem
{
    public string UserRef { get; set; }
    public string Email { get; set; }
    public string Role { get; set; }
    public bool CanReceiveNotifications { get; set; }
    public string Name { get; set; }
    public int Status { get; set; }
}