using System;
using System.Threading.Tasks;
using SFA.DAS.Reservations.Application.Validation;

namespace SFA.DAS.Reservations.Application.Reservations.Commands
{
    public class CacheCreateReservationCommandValidator : IValidator<CacheCreateReservationCommand>
    {
        public Task<ValidationResult> ValidateAsync(CacheCreateReservationCommand command)
        {
            var result = new ValidationResult();

            if (command.AccountId == default(long))
            {
                result.AddError(nameof(command.AccountId));
            }

            if (string.IsNullOrEmpty(command.StartDate))
            {
                if (command.Id.HasValue)
                {
                    result.AddError(nameof(command.StartDate));
                }
            }
            else
            {
                var dateSplit = command.StartDate.Split("-");

                if (dateSplit.Length != 2)
                {
                    result.AddError(nameof(command.StartDate));
                    return Task.FromResult(result);
                }

                var yearValid = int.TryParse(dateSplit[0], out var year);
                var monthValid = int.TryParse(dateSplit[1], out var month);

                if (!yearValid || !monthValid)
                {
                    result.AddError(nameof(command.StartDate));
                    return Task.FromResult(result);
                }

                var startDate = DateTime.TryParse($"{year}-{month}-01", out var parseDateTime);

                if (!startDate || parseDateTime.Year != year || parseDateTime.Month != month)
                {
                    result.AddError(nameof(command.StartDate));
                }
            }
            

            return Task.FromResult(result);
        }
    }
}
