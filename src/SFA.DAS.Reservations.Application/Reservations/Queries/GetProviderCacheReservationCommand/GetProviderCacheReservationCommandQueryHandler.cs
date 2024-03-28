using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.Reservations.Application.Commitments.Queries.GetCohort;
using SFA.DAS.Reservations.Application.Exceptions;
using SFA.DAS.Reservations.Application.Extensions;
using SFA.DAS.Reservations.Application.Providers.Queries.GetLegalEntityAccount;
using SFA.DAS.Reservations.Application.Providers.Queries.GetTrustedEmployers;
using SFA.DAS.Reservations.Application.Reservations.Commands.CacheReservationEmployer;
using SFA.DAS.Reservations.Application.Validation;

namespace SFA.DAS.Reservations.Application.Reservations.Queries.GetProviderCacheReservationCommand
{
    public class GetProviderCacheReservationCommandQueryHandler : IRequestHandler<GetProviderCacheReservationCommandQuery, GetProviderCacheReservationCommandResponse>
    {
        private readonly IMediator _mediator;
        private readonly IValidator<GetProviderCacheReservationCommandQuery> _validator;

        public GetProviderCacheReservationCommandQueryHandler(IMediator mediator,
            IValidator<GetProviderCacheReservationCommandQuery> validator)
        {
            _mediator = mediator;
            _validator = validator;
        }

        public async Task<GetProviderCacheReservationCommandResponse> Handle(
            GetProviderCacheReservationCommandQuery query, 
            CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(query);

            if (!validationResult.IsValid())
            {
                throw new ValidationException(validationResult.ConvertToDataAnnotationsValidationResult(), null, null);
            }

            var accounts = await _mediator.Send(
                new GetTrustedEmployersQuery { UkPrn = query.UkPrn }, cancellationToken);
            
            var matchedAccount = accounts.Employers.SingleOrDefault(employer =>
                employer.AccountLegalEntityPublicHashedId == query.AccountLegalEntityPublicHashedId);

            if (matchedAccount != null)
            {
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
           
            var result = await _mediator.Send(new GetAccountLegalEntityQuery
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
                var cohort = await _mediator.Send(new GetCohortQuery { CohortId = query.CohortId.Value }, cancellationToken);

                if (cohort.Cohort.AccountLegalEntityId != legalEntity.AccountLegalEntityId)
                {
                    throw new ProviderNotAuthorisedException(legalEntity.AccountId, query.UkPrn);
                }
            }

            return new GetProviderCacheReservationCommandResponse
            {
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
