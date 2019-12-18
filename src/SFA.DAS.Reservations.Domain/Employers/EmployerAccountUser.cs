using System.Threading;
using SFA.DAS.EAS.Account.Api.Types;

namespace SFA.DAS.Reservations.Domain.Employers
{
    public class EmployerAccountUser
    {
        public string UserRef { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public bool CanReceiveNotifications { get; set; }
        public int Status { get; set; }

        public static implicit operator EmployerAccountUser(TeamMemberViewModel source)
        {
            return new EmployerAccountUser
            {
                UserRef = source.UserRef,
                Name = source.Name,
                Email = source.Email,
                Role = source.Role,
                CanReceiveNotifications = source.CanReceiveNotifications,
                Status = (int)source.Status
            };
        }
    }
}