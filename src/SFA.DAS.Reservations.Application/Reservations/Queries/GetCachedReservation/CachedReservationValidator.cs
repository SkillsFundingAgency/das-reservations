using System;
using System.Threading.Tasks;
using SFA.DAS.Reservations.Application.Validation;
using SFA.DAS.Reservations.Domain.Reservations;

namespace SFA.DAS.Reservations.Application.Reservations.Queries.GetCachedReservation
{
    public class CachedReservationValidator : IValidator<CachedReservation>
    {
        public Task<ValidationResult> ValidateAsync(CachedReservation reservation)
        {
            var result = new ValidationResult();

            if (reservation.AccountId == default(long))
            {
                result.AddError(nameof(reservation.AccountId));
            }
            if (string.IsNullOrEmpty(reservation.AccountLegalEntityName))
            {
                result.AddError(nameof(reservation.AccountLegalEntityName));
            }

            if (reservation.TrainingDate == null)
            {
                result.AddError(nameof(reservation.TrainingDate));
            }
            else if (reservation.TrainingDate.StartDate == DateTime.MinValue)
            {
                result.AddError(nameof(reservation.TrainingDate), $"{nameof(reservation.TrainingDate.StartDate)} must be set on {nameof(reservation.TrainingDate)}");
            }

            return Task.FromResult(result);
        }
    }
}
