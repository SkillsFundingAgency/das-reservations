using System;
using System.Threading.Tasks;
using SFA.DAS.Reservations.Application.Validation;

namespace SFA.DAS.Reservations.Application.Reservations.Commands
{
    public class CreateReservationCommandValidator : IValidator<ICreateReservationCommand>
    {
        public Task<ValidationResult> ValidateAsync(ICreateReservationCommand query)
        {
            var result = new ValidationResult();

            if (query.AccountId == "0")
            {
                result.AddError(nameof(query.AccountId));
            }

            if (string.IsNullOrEmpty(query.StartDate))
            {
                result.AddError(nameof(query.StartDate));
            }
            else
            {
                var dateSplit = query.StartDate.Split("-");

                if (dateSplit.Length != 2)
                {
                    result.AddError(nameof(query.StartDate));
                    return Task.FromResult(result);
                }

                var yearValid = int.TryParse(dateSplit[0], out var year);
                var monthValid = int.TryParse(dateSplit[1], out var month);

                if (!yearValid || !monthValid)
                {
                    result.AddError(nameof(query.StartDate));
                    return Task.FromResult(result);
                }

                var startDate = DateTime.TryParse($"{year}-{month}-01", out var parseDateTime);

                if (!startDate || parseDateTime.Year != year || parseDateTime.Month != month)
                {
                    result.AddError(nameof(query.StartDate));
                }
            }
            

            return Task.FromResult(result);
        }
    }
}