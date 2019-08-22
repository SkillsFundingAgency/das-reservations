using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;
using SFA.DAS.Reservations.Application.Extensions;
using SFA.DAS.Reservations.Application.Validation;
using SFA.DAS.Reservations.Domain.Interfaces;
using ValidationResult = System.ComponentModel.DataAnnotations.ValidationResult;

namespace SFA.DAS.Reservations.Application.Employers.Queries
{
    public class GetTrustedEmployersQueryHandler : IRequestHandler<GetTrustedEmployersQuery, GetTrustedEmployersResponse>
    {
        private readonly IProviderPermissionsService _providerPermissionsService;
        private readonly IValidator<GetTrustedEmployersQuery> _validator;

        public GetTrustedEmployersQueryHandler(IProviderPermissionsService providerPermissionsService,
            IValidator<GetTrustedEmployersQuery> validator)
        {
            _providerPermissionsService = providerPermissionsService;
            _validator = validator;
        }

        public async Task<GetTrustedEmployersResponse> Handle(GetTrustedEmployersQuery request, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request);

            if (!validationResult.IsValid())
            {
                throw new ValidationException(validationResult.ConvertToDataAnnotationsValidationResult(), null, null);
            }

            var trustedEmployers = await _providerPermissionsService.GetTrustedEmployers(request.UkPrn);

            return new GetTrustedEmployersResponse
            {
                Employers = trustedEmployers
            };
        }
    }
}
