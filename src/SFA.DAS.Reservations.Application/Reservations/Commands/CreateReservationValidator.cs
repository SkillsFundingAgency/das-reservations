using System;
using System.Threading.Tasks;
using SFA.DAS.Reservations.Application.Validation;

namespace SFA.DAS.Reservations.Application.Reservations.Commands
{
    public class CreateReservationValidator : IValidator<BaseCreateReservationCommand>
    {
        public Task<ValidationResult> ValidateAsync(BaseCreateReservationCommand item)
        {
            var result = new ValidationResult();

            if (item.AccountId == "0")
            {
                result.AddError(nameof(item.AccountId));
            }

            if (string.IsNullOrEmpty(item.StartDate))
            {
                result.AddError(nameof(item.StartDate));
            }
            else
            {
                var dateSplit = item.StartDate.Split("-");

                if (dateSplit.Length != 2)
                {
                    result.AddError(nameof(item.StartDate));
                    return Task.FromResult(result);
                }

                var yearValid = int.TryParse(dateSplit[0], out var year);
                var monthValid = int.TryParse(dateSplit[1], out var month);

                if (!yearValid || !monthValid)
                {
                    result.AddError(nameof(item.StartDate));
                    return Task.FromResult(result);
                }

                var startDate = DateTime.TryParse($"{year}-{month}-01", out var parseDateTime);

                if (!startDate || parseDateTime.Year != year || parseDateTime.Month != month)
                {
                    result.AddError(nameof(item.StartDate));
                }
            }
            

            return Task.FromResult(result);
        }
    }
}