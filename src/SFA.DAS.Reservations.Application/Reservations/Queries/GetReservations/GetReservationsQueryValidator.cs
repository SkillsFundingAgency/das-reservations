using System.Threading.Tasks;
using SFA.DAS.Reservations.Application.Validation;

namespace SFA.DAS.Reservations.Application.Reservations.Queries.GetReservations
{
    public class GetReservationsQueryValidator : IValidator<GetReservationsQuery>
    {
        public Task<ValidationResult> ValidateAsync(GetReservationsQuery query)
        {
            var result = new ValidationResult();

            if (query.AccountId < 1)
            {
                result.AddError(nameof(query.AccountId));
            }
            
            return Task.FromResult(result);
        }
    }
}