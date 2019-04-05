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

            if (string.IsNullOrEmpty(reservation.StartDate))
            {
                result.AddError(nameof(reservation.StartDate));
            }
            else
            {
                var dateSplit = reservation.StartDate.Split("-");

                if (dateSplit.Length != 2)
                {
                    result.AddError(nameof(reservation.StartDate));
                    return Task.FromResult(result);
                }

                var yearValid = int.TryParse(dateSplit[0], out var year);
                var monthValid = int.TryParse(dateSplit[1], out var month);

                if (!yearValid || !monthValid)
                {
                    result.AddError(nameof(reservation.StartDate));
                    return Task.FromResult(result);
                }

                var startDate = DateTime.TryParse($"{year}-{month}-01", out var parseDateTime);

                if (!startDate || parseDateTime.Year != year || parseDateTime.Month != month)
                {
                    result.AddError(nameof(reservation.StartDate));
                }
            }

            return Task.FromResult(result);
        }
    }
}
