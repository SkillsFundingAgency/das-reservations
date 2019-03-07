using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.Reservations.Application.Validation;

namespace SFA.DAS.Reservations.Application.Reservations.Commands
{
    public class CacheReservationCommandHandler : IRequestHandler<CacheReservationCommand, CacheReservationResult>
    {
        private readonly IValidator<BaseCreateReservationCommand> _validator;

        public CacheReservationCommandHandler(IValidator<BaseCreateReservationCommand> validator)
        {
            _validator = validator;
        }

        public async Task<CacheReservationResult> Handle(CacheReservationCommand command, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(command);
            var result = new CacheReservationResult();
            await Task.CompletedTask;
            return result;
        }
    }
}