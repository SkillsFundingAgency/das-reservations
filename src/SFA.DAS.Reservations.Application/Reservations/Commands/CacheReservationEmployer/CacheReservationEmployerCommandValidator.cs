using System;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.Reservations.Application.Employers.Queries;
using SFA.DAS.Reservations.Application.Validation;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Domain.Rules;

namespace SFA.DAS.Reservations.Application.Reservations.Commands.CacheReservationEmployer
{
    public class CacheReservationEmployerCommandValidator : IValidator<CacheReservationEmployerCommand>
    {
        private IFundingRulesService _rulesService;
        private readonly IMediator _mediator;

        public CacheReservationEmployerCommandValidator(IFundingRulesService rulesService, IMediator mediator)
        {
            _rulesService = rulesService;
            _mediator = mediator;
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
                var accountFundingRulesApiResponse = await _rulesService.GetAccountFundingRules(command.AccountId);
                if (accountFundingRulesApiResponse.GlobalRules.Any(c => c != null && c.RuleType == GlobalRuleType.ReservationLimit) &&
                    accountFundingRulesApiResponse.GlobalRules.Count(c => c.RuleType == GlobalRuleType.ReservationLimit) > 0)
                {
                    result.FailedRuleValidation = true;
                }

                var globalRulesApiResponse = await _rulesService.GetFundingRules();
                if (globalRulesApiResponse.GlobalRules != null && globalRulesApiResponse.GlobalRules.Any(c => c != null && c.RuleType == GlobalRuleType.FundingPaused) &&
                    globalRulesApiResponse.GlobalRules.Count(c => c.RuleType == GlobalRuleType.FundingPaused) > 0)
                {
                    result.FailedGlobalRuleValidation = true;
                }

                // todo: eoi check, throw exception if fails
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

            if (command.UkPrn.HasValue)
            {
                var accounts = await _mediator.Send(
                    new GetTrustedEmployersQuery { UkPrn = command.UkPrn.Value });
                
                var matchedAccount = accounts?.Employers?.SingleOrDefault(employer =>
                    employer.AccountLegalEntityPublicHashedId == command.AccountLegalEntityPublicHashedId);

                result.FailedAuthorisationValidation = matchedAccount == null;
            }

            return result;
        }
    }
}
