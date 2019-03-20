using System;
using System.Threading.Tasks;
using SFA.DAS.Reservations.Application.Validation;

namespace SFA.DAS.Reservations.Application.Reservations.Queries.GetReservation
{
    public class GetReservationQueryValidator : IValidator<IReservationQuery>
    {
        public  Task<ValidationResult> ValidateAsync(IReservationQuery item)
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