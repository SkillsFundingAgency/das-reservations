using System;
using System.Threading.Tasks;
using SFA.DAS.Reservations.Application.Validation;

namespace SFA.DAS.Reservations.Application.Reservations.Queries
{
    public class GetReservationQueryValidator : IValidator<GetReservationQuery>
    {
        public  Task<ValidationResult> ValidateAsync(GetReservationQuery item)
        {
            var result = new ValidationResult();

            if (item.Id == Guid.Empty)
            {
                result.AddError(nameof(item.Id));
            }

            return Task.FromResult(result);
        }
    }
}