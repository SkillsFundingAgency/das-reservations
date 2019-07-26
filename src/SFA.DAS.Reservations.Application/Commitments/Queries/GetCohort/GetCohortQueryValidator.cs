using System.Threading.Tasks;
using SFA.DAS.Reservations.Application.Validation;

namespace SFA.DAS.Reservations.Application.Commitments.Queries.GetCohort
{
    public class GetCohortQueryValidator : IValidator<GetCohortQuery>
    {
        public Task<ValidationResult> ValidateAsync(GetCohortQuery query)
        {
           var validationResult = new ValidationResult();

           if (query.CohortId == default(long))
           {
               validationResult.AddError(nameof(query.CohortId));
           }

           return Task.FromResult(validationResult);
        }
    }
}
