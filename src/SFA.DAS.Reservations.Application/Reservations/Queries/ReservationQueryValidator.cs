using System;
using System.Threading.Tasks;
using SFA.DAS.Reservations.Application.Validation;

namespace SFA.DAS.Reservations.Application.Reservations.Queries
{
    public class ReservationQueryValidator : IValidator<IReservationQuery>
    {
        public  Task<ValidationResult> ValidateAsync(IReservationQuery query)
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