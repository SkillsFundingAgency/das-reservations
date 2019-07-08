using System.Threading.Tasks;
using SFA.DAS.Reservations.Application.Validation;

namespace SFA.DAS.Reservations.Application.Reservations.Queries.GetAvailableReservations
{
    public class GetAvailableReservationsQueryValidator : IValidator<GetAvailableReservationsQuery>
    {
        public Task<ValidationResult> ValidateAsync(GetAvailableReservationsQuery query)
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