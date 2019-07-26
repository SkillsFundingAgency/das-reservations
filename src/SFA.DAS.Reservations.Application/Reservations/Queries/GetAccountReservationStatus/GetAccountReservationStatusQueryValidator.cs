using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using SFA.DAS.Reservations.Application.Validation;

namespace SFA.DAS.Reservations.Application.Reservations.Queries.GetAccountReservationStatus
{
    public class GetAccountReservationStatusQueryValidator : IValidator<GetAccountReservationStatusQuery>

    {
        public Task<ValidationResult> ValidateAsync(GetAccountReservationStatusQuery query)
        {
            var validationResult = new ValidationResult();

            if (query.AccountId < 1)
            {
                validationResult.AddError(nameof(query.AccountId));
            }

            return Task.FromResult(validationResult);
        }
    }
}
