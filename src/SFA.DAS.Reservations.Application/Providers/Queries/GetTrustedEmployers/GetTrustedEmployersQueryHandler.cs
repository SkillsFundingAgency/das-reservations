using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.Encoding;
using SFA.DAS.Reservations.Application.Extensions;
using SFA.DAS.Reservations.Application.Providers.Services;
using SFA.DAS.Reservations.Application.Validation;
using SFA.DAS.Reservations.Domain.Interfaces;
using ValidationResult = System.ComponentModel.DataAnnotations.ValidationResult;

namespace SFA.DAS.Reservations.Application.Providers.Queries.GetTrustedEmployers
{
    public class GetTrustedEmployersQueryHandler : IRequestHandler<GetTrustedEmployersQuery, GetTrustedEmployersResponse>
    {
        private readonly IProviderService _providerService;
        private readonly IEncodingService _encodingService;
        private readonly IValidator<GetTrustedEmployersQuery> _validator;

        public GetTrustedEmployersQueryHandler(IProviderService providerService,
            IEncodingService encodingService,
                IValidator<GetTrustedEmployersQuery> validator)
        {
            _providerService = providerService;
            _encodingService = encodingService;
            _validator = validator;
        }

        public async Task<GetTrustedEmployersResponse> Handle(GetTrustedEmployersQuery request, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request);

            if (!validationResult.IsValid())
            {
                throw new ValidationException(validationResult.ConvertToDataAnnotationsValidationResult(), null, null);
            }

            var trustedEmployers = (await _providerService.GetTrustedEmployers(request.UkPrn)).ToList();

            foreach (var trustedEmployer in trustedEmployers)
            {
                trustedEmployer.AccountPublicHashedId =
                    _encodingService.Encode(trustedEmployer.AccountId, EncodingType.PublicAccountId);
                trustedEmployer.AccountLegalEntityPublicHashedId =
                    _encodingService.Encode(trustedEmployer.AccountLegalEntityId, EncodingType.PublicAccountLegalEntityId);
            }
            
            return new GetTrustedEmployersResponse
            {
                Employers = trustedEmployers
            };
        }
    }
}
