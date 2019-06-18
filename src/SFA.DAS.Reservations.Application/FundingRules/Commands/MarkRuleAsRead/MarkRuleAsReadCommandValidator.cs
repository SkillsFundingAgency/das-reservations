using System;
using System.Threading.Tasks;
using SFA.DAS.Reservations.Application.Validation;
using SFA.DAS.Reservations.Domain.Rules.Api;

namespace SFA.DAS.Reservations.Application.FundingRules.Commands.MarkRuleAsRead
{
    public class MarkRuleAsReadCommandValidator : IValidator<MarkRuleAsReadCommand>
    {
        public Task<ValidationResult> ValidateAsync(MarkRuleAsReadCommand command)
        {
            var validationResults = new ValidationResult();

            if (string.IsNullOrEmpty(command.Id))
            {
                validationResults.AddError(nameof(command.Id), $"{nameof( MarkRuleAsReadCommand.Id)} has not been supplied");
            }
            else if (!Guid.TryParse(command.Id, out _) && !long.TryParse(command.Id, out _))
            {
                validationResults.AddError(nameof(command.Id), $"{nameof( MarkRuleAsReadCommand.Id)} value is invalid");
            }

            if (command.RuleId == 0)
            {
                validationResults.AddError(nameof(command.RuleId), $"{nameof( MarkRuleAsReadCommand.RuleId)} has not been supplied");
            }
            else if (command.RuleId < 0)
            {
                validationResults.AddError(nameof(command.RuleId), $"{nameof( MarkRuleAsReadCommand.RuleId)} value is invalid");
            }

            if (command.TypeOfRule == RuleType.None)
            {
                validationResults.AddError(nameof(command.TypeOfRule), $"{nameof( MarkRuleAsReadCommand.TypeOfRule)} has not been supplied");
            }

            return Task.FromResult(validationResults);
        }
    }
}
