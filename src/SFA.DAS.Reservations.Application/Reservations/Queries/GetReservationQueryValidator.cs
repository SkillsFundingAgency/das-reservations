using System.Threading.Tasks;
using SFA.DAS.Reservations.Application.Validation;

namespace SFA.DAS.Reservations.Application.Reservations.Queries
{
    public class GetReservationQueryValidator : IValidator<GetReservationQuery>
    {
        public Task<ValidationResult> ValidateAsync(GetReservationQuery item)
        {
            throw new System.NotImplementedException();
        }
    }
}