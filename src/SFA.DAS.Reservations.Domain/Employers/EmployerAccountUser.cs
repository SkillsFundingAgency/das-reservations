using SFA.DAS.Reservations.Domain.Employers.Api;

namespace SFA.DAS.Reservations.Domain.Employers;

public class EmployerAccountUser
{
    public string UserRef { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string Role { get; set; }
    public bool CanReceiveNotifications { get; set; }
    public int Status { get; set; }
    
    public static implicit operator EmployerAccountUser(AccountUsersResponseItem source)
    {
        return new EmployerAccountUser
        {
            UserRef = source.UserRef,
            Name = source.Name,
            Email = source.Email,
            Role = source.Role,
            CanReceiveNotifications = source.CanReceiveNotifications,
            Status = source.Status
        };
    }
}