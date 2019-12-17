using SFA.DAS.Reservations.Domain.Employers;

namespace SFA.DAS.Reservations.Web.Models
{
    public class EmployerAccountUserViewModel
    {
        public string Name { get; set; }
        public string Email { get; set; }

        public static implicit operator EmployerAccountUserViewModel(EmployerAccountUser source)
        {
            return new EmployerAccountUserViewModel
            {
                Name = source.Name,
                Email = source.Email
            };
        }
    }
}