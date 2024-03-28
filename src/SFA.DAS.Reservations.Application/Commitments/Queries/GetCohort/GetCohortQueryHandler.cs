using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.Reservations.Application.Commitments.Services;
using SFA.DAS.Reservations.Application.Extensions;
using SFA.DAS.Reservations.Application.Validation;

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
                throw new ValidationException(validationResult.ConvertToDataAnnotationsValidationResult(), null, null);
            }

            var cohort = await _service.GetCohort(query.CohortId);

            return new GetCohortResponse{ Cohort = cohort };
        }
    }
}
