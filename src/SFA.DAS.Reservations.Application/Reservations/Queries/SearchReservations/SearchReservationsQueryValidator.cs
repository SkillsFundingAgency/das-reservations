using System.Threading.Tasks;
using SFA.DAS.Reservations.Application.Validation;

namespace SFA.DAS.Reservations.Application.Reservations.Queries.SearchReservations
{
    public class SearchReservationsQueryValidator : IValidator<SearchReservationsQuery>
    {
        public Task<ValidationResult> ValidateAsync(SearchReservationsQuery query)
        {
            return Task.FromResult(
                new ValidationResult());
        }
    }
}