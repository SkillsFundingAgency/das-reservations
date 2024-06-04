using System.Linq;
using System.Threading.Tasks;
using SFA.DAS.Reservations.Application.Validation;
using SFA.DAS.Reservations.Domain.Interfaces;

namespace SFA.DAS.Reservations.Application.Reservations.Queries.SearchReservations
{
    public class SearchReservationsQueryValidator : IValidator<SearchReservationsQuery>
    {
        private readonly IReservationsOuterService _reservationsOuterService;

        public SearchReservationsQueryValidator(IReservationsOuterService reservationsOuterService)
        {
            _reservationsOuterService = reservationsOuterService;
        }

        public async Task<ValidationResult> ValidateAsync(SearchReservationsQuery query)
        {
            var validationResult = new ValidationResult();

            if (query.ProviderId == 0)
            {
                validationResult.AddError(nameof(query.ProviderId));
                return validationResult;
            }

            var result = await _reservationsOuterService.GetTrustedEmployers(query.ProviderId);

            if (result == null || !result.Any())
            {
                validationResult.FailedAuthorisationValidation = true;
            }

            return validationResult;
        }
    }
}