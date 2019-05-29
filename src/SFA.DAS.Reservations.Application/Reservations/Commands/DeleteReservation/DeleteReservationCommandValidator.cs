using System;
using System.Threading.Tasks;
using SFA.DAS.Reservations.Application.Validation;

namespace SFA.DAS.Reservations.Application.Reservations.Commands.DeleteReservation
{
    public class DeleteReservationCommandValidator : IValidator<DeleteReservationCommand>
    {
        public Task<ValidationResult> ValidateAsync(DeleteReservationCommand command)
        {
            var result = new ValidationResult();

            if (command.ReservationId == Guid.Empty)
            {
                result.AddError(nameof(command.ReservationId));
            }

            return Task.FromResult(result);
        }
    }
}