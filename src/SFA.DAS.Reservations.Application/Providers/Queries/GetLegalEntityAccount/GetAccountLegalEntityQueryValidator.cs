using System;
using System.Threading.Tasks;
using SFA.DAS.Reservations.Application.Validation;

namespace SFA.DAS.Reservations.Application.Providers.Queries.GetLegalEntityAccount
{
    public class GetAccountLegalEntityQueryValidator : IValidator<GetAccountLegalEntityQuery>
    {
        public Task<ValidationResult> ValidateAsync(GetAccountLegalEntityQuery query)
        {
            throw new NotImplementedException();
        }
    }
}
