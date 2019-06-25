using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SFA.DAS.Reservations.Application.Validation;

namespace SFA.DAS.Reservations.Application.Reservations.Commands.CreateReservationLevyEmployer
{
    public class CreateReservationLevyEmployerCommandValidator : IValidator<CreateReservationLevyEmployerCommand>
    {
        public Task<ValidationResult> ValidateAsync(CreateReservationLevyEmployerCommand query)
        {
            throw new NotImplementedException();
        }
    }
}
