using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.Reservations.Application.Validation;

namespace SFA.DAS.Reservations.Application.Reservations.Commands
{
    public class CreateReservationCommandHandler : IRequestHandler<CreateReservationCommand, CreateReservationResult>
    {
        private readonly IValidator<CreateReservationCommand> _validator;

        public CreateReservationCommandHandler(IValidator<CreateReservationCommand> validator)
        {
            _validator = validator;
        }

        public async Task<CreateReservationResult> Handle(CreateReservationCommand request, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request);

            if (!validationResult.IsValid())
            {
                throw new ArgumentException(
                    "The following parameters have failed validation", 
                    validationResult.ValidationDictionary
                        .Select(c => c.Key)
                        .Aggregate((item1, item2) => item1 + ", " + item2));
            }

            return new CreateReservationResult();
        }
    }
}