using System;
using System.Threading.Tasks;
using SFA.DAS.Reservations.Application.Validation;

namespace SFA.DAS.Reservations.Application.Reservations.Queries
{
    public class GetReservationQueryValidator : IValidator<GetReservationQuery>
    {
        public  Task<ValidationResult> ValidateAsync(GetReservationQuery query)
        {
            var result = new ValidationResult();

            if (query.Id == Guid.Empty)
            {
                result.AddError(nameof(query.Id));
            }

            return Task.FromResult(result);
        }
    }
}