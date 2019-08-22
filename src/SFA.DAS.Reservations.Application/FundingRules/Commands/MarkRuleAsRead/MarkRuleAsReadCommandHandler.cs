using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.Reservations.Application.Extensions;
using SFA.DAS.Reservations.Application.Validation;
using SFA.DAS.Reservations.Domain.Interfaces;
using ValidationResult = System.ComponentModel.DataAnnotations.ValidationResult;

namespace SFA.DAS.Reservations.Application.FundingRules.Commands.MarkRuleAsRead
{
    public class MarkRuleAsReadCommandHandler : IRequestHandler<MarkRuleAsReadCommand, Unit>
    {
        private readonly IFundingRulesService _service;
        private readonly IValidator<MarkRuleAsReadCommand> _validator;

        public MarkRuleAsReadCommandHandler(IFundingRulesService service, IValidator<MarkRuleAsReadCommand> validator)
        {
            _service = service;
            _validator = validator;
        }

        public async Task<Unit> Handle(MarkRuleAsReadCommand command, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(command);

            if (!validationResult.IsValid())
            {
                throw new ValidationException(validationResult.ConvertToDataAnnotationsValidationResult(), null, null);
            }

            await _service.MarkRuleAsRead(command.Id, command.RuleId, command.TypeOfRule);

            return Unit.Value;
        }
    }
}
