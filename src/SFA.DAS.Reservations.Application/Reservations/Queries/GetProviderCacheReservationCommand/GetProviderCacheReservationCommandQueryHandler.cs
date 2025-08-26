using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using SFA.DAS.Reservations.Application.Commitments.Queries.GetCohort;
using SFA.DAS.Reservations.Application.Exceptions;
using SFA.DAS.Reservations.Application.Extensions;
using SFA.DAS.Reservations.Application.Providers.Queries.GetLegalEntityAccount;
using SFA.DAS.Reservations.Application.Providers.Queries.GetTrustedEmployers;
using SFA.DAS.Reservations.Application.Reservations.Commands.CacheReservationEmployer;
using SFA.DAS.Reservations.Application.Validation;

namespace SFA.DAS.Reservations.Application.Reservations.Queries.GetProviderCacheReservationCommand
{
    public class GetProviderCacheReservationCommandQueryHandler(
        IMediator mediator,
        IValidator<GetProviderCacheReservationCommandQuery> validator,
        ILogger<GetProviderCacheReservationCommandQueryHandler> logger)
        : IRequestHandler<GetProviderCacheReservationCommandQuery, GetProviderCacheReservationCommandResponse>
    {
        public async Task<GetProviderCacheReservationCommandResponse> Handle(
            GetProviderCacheReservationCommandQuery query, 
            CancellationToken cancellationToken)
        {
            var validationResult = await validator.ValidateAsync(query);

            if (!validationResult.IsValid())
            {
                throw new ValidationException(validationResult.ConvertToDataAnnotationsValidationResult(), null, null);
            }

            var accounts = await mediator.Send(
                new GetTrustedEmployersQuery { UkPrn = query.UkPrn }, cancellationToken);
            
            var matchedAccount = accounts.Employers.SingleOrDefault(employer =>
                employer.AccountLegalEntityPublicHashedId == query.AccountLegalEntityPublicHashedId);

            if (matchedAccount != null)
            {
                logger.LogInformation("Matched Employer Legal Entity from trusted list, {0}", matchedAccount.AccountId);
                return new GetProviderCacheReservationCommandResponse
                {
                    Command = new CacheReservationEmployerCommand
                    {
                        AccountLegalEntityName = matchedAccount.AccountLegalEntityName,
                        AccountLegalEntityPublicHashedId = matchedAccount.AccountLegalEntityPublicHashedId,
                        UkPrn = query.UkPrn,
                        AccountLegalEntityId = matchedAccount.AccountLegalEntityId,
                        Id = Guid.NewGuid(),
                        CohortRef = query.CohortRef,
                        AccountId = matchedAccount.AccountId,
                        AccountName = matchedAccount.AccountName
                    }
                };
            }

            logger.LogInformation("Looking For Legal Entity by query.AccountLegalEntityPublicHashedId, {0}", query.AccountLegalEntityPublicHashedId);
            var result = await mediator.Send(new GetAccountLegalEntityQuery
            {
                AccountLegalEntityPublicHashedId = query.AccountLegalEntityPublicHashedId
            }, cancellationToken);

            var legalEntity = result?.LegalEntity;

            if (legalEntity == null)
            {
                throw new AccountLegalEntityNotFoundException(query.AccountLegalEntityPublicHashedId);
            }

            if (query.CohortId.HasValue)
            {
                logger.LogInformation("Looking For Legal Entity by query.CohortId, {0}", query.CohortId);
                var cohort = await mediator.Send(new GetCohortQuery { CohortId = query.CohortId.Value }, cancellationToken);

                if (cohort.Cohort.AccountLegalEntityId != legalEntity.AccountLegalEntityId)
                {
                    throw new ProviderNotAuthorisedException(legalEntity.AccountId, query.UkPrn);
                }
            }

            logger.LogInformation("Legal entity: AccountLegalEntityName {0}, AccountLegalEntityPublicHashedId {1}, AccountId {2} ", 
                legalEntity.AccountLegalEntityName, legalEntity.AccountLegalEntityPublicHashedId, legalEntity.AccountId);

            return new GetProviderCacheReservationCommandResponse
            {
                LegalEntity = legalEntity,
                Command = new CacheReservationEmployerCommand
                {
                    AccountLegalEntityName = legalEntity.AccountLegalEntityName,
                    AccountLegalEntityPublicHashedId = query.AccountLegalEntityPublicHashedId,
                    UkPrn = query.UkPrn,
                    AccountLegalEntityId = legalEntity.AccountLegalEntityId,
                    Id = Guid.NewGuid(),
                    CohortRef = query.CohortRef,
                    AccountId = legalEntity.AccountId
                }
            };
        }
    }
}
