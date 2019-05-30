using System;
using System.Linq;
using System.Threading.Tasks;
using SFA.DAS.Reservations.Application.Validation;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Domain.Rules;

namespace SFA.DAS.Reservations.Application.Reservations.Commands.CacheReservationEmployer
{
    public class CacheReservationEmployerCommandValidator : IValidator<CacheReservationEmployerCommand>
    {
        private IFundingRulesService _rulesService;

        public CacheReservationEmployerCommandValidator(IFundingRulesService rulesService)
        {
            _rulesService = rulesService;
        }

        public async Task<ValidationResult> ValidateAsync(CacheReservationEmployerCommand command)
        {
            var result = new ValidationResult();

            if (command.Id == Guid.Empty)
            {
                result.AddError(nameof(command.Id));
            }

            if (command.AccountId == default(long))
            {
                result.AddError(nameof(command.AccountId));
            }
            else
            {
                var globalRules = await _rulesService.GetAccountFundingRules(command.AccountId);
                if (globalRules.GlobalRules.Any(c => c != null && c.RuleType == GlobalRuleType.ReservationLimit) &&
                    globalRules.GlobalRules.Count(c => c.RuleType == GlobalRuleType.ReservationLimit) > 0)
                {
                    result.FailedRuleValidation = true;
                }
            }

            if (command.AccountLegalEntityId == default(long))
            {
                result.AddError(nameof(command.AccountLegalEntityId));
            }

            if (string.IsNullOrWhiteSpace(command.AccountLegalEntityName))
            {
                result.AddError(nameof(command.AccountLegalEntityName));
            }

            if (string.IsNullOrWhiteSpace(command.AccountLegalEntityPublicHashedId))
            {
                result.AddError(nameof(command.AccountLegalEntityPublicHashedId));
            }

            return result;
        }
    }
}