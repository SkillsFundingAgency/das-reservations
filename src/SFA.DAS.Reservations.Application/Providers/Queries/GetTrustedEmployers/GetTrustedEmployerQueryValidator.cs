using System.Threading.Tasks;
using SFA.DAS.Reservations.Application.Validation;

namespace SFA.DAS.Reservations.Application.Providers.Queries.GetTrustedEmployers
{
    public class GetTrustedEmployerQueryValidator : IValidator<GetTrustedEmployersQuery>
    {
        public Task<ValidationResult> ValidateAsync(GetTrustedEmployersQuery query)
        {
            var result = new ValidationResult();

            if (query.UkPrn == default(uint))
            {
                result.AddError(nameof(query.UkPrn));
            }

            return Task.FromResult(result);
        }
    }
}
