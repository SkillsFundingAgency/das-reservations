using System;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using SFA.DAS.Reservations.Application.Providers.Queries.GetTrustedEmployers;
using SFA.DAS.Reservations.Application.Validation;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Domain.Rules;

namespace SFA.DAS.Reservations.Application.Reservations.Commands.CacheReservationEmployer;

public class CacheReservationEmployerCommandValidator(
    IFundingRulesService rulesService,
    IMediator mediator,
    ILogger<CacheReservationEmployerCommandValidator> logger)
    : IValidator<CacheReservationEmployerCommand>
{
    private ILogger<CacheReservationEmployerCommandValidator> _logger = logger;

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
            var accountFundingRulesApiResponse = await rulesService.GetAccountFundingRules(command.AccountId);
            if (accountFundingRulesApiResponse.GlobalRules.Any(c => c != null && c.RuleType == GlobalRuleType.ReservationLimit) &&
                accountFundingRulesApiResponse.GlobalRules.Count(c => c.RuleType == GlobalRuleType.ReservationLimit) > 0)
            {
                _logger.LogWarning("Account {AccountId} has reached the reservation limit.", command.AccountId);
                result.FailedRuleValidation = true;
            }

            var globalRulesApiResponse = await rulesService.GetFundingRules();
            if (globalRulesApiResponse.GlobalRules != null 
                && globalRulesApiResponse.GlobalRules.Any(c => c != null && c.RuleType == GlobalRuleType.FundingPaused) 
                && globalRulesApiResponse.GlobalRules.Count(c => c.RuleType == GlobalRuleType.FundingPaused && DateTime.UtcNow >= c.ActiveFrom) > 0)
            {
                result.FailedGlobalRuleValidation = true;
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

        if (command.UkPrn.HasValue && !command.IsEmptyCohortFromSelect)
        {
            var accounts = await mediator.Send(
                new GetTrustedEmployersQuery { UkPrn = command.UkPrn.Value });
                
            var matchedAccount = accounts?.Employers?.SingleOrDefault(employer =>
                employer.AccountLegalEntityPublicHashedId == command.AccountLegalEntityPublicHashedId);

            result.FailedAuthorisationValidation = matchedAccount == null;
        }

        return result;
    }
}