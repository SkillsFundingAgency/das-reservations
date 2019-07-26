using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.Reservations.Application.Commitments.Services;
using SFA.DAS.Reservations.Application.Validation;
using ValidationResult = System.ComponentModel.DataAnnotations.ValidationResult;

namespace SFA.DAS.Reservations.Application.Commitments.Queries.GetCohort
{
    public class GetCohortQueryHandler : IRequestHandler<GetCohortQuery, GetCohortResponse>
    {
        private readonly ICommitmentService _service;
        private readonly IValidator<GetCohortQuery> _validator;

        public GetCohortQueryHandler(ICommitmentService service, IValidator<GetCohortQuery> validator)
        {
            _service = service;
            _validator = validator;
        }

        public async Task<GetCohortResponse> Handle(GetCohortQuery query, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(query);

            if (!validationResult.IsValid())
            {
                throw new ValidationException(
                    new ValidationResult("The following parameters have failed validation", validationResult.ErrorList), null, null);
            }

            var cohort = await _service.GetCohort(query.CohortId);

            return new GetCohortResponse{ Cohort = cohort };
        }
    }
}
