using System.Threading.Tasks;
using SFA.DAS.Reservations.Application.Validation;

namespace SFA.DAS.Reservations.Application.Reservations.Queries
{
    public class GetReservationCommandValidator : IValidator<GetReservationCommand>
    {
        public Task<ValidationResult> ValidateAsync(GetReservationCommand item)
        {
            throw new System.NotImplementedException();
        }
    }
}