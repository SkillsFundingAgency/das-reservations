using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.Encoding;
using SFA.DAS.Reservations.Application.Extensions;
using SFA.DAS.Reservations.Application.Providers.Services;
using SFA.DAS.Reservations.Application.Validation;


namespace SFA.DAS.Reservations.Application.Providers.Queries.GetLegalEntityAccount
{
    public class GetAccountLegalEntityQueryHandler : IRequestHandler<GetAccountLegalEntityQuery, GetAccountLegalEntityResult>
    {
        private readonly IProviderService _providerService;
        private readonly IEncodingService _encodingService;
        private readonly IValidator<GetAccountLegalEntityQuery> _validator;

        public GetAccountLegalEntityQueryHandler(
            IProviderService providerService, 
            IEncodingService encodingService,
            IValidator<GetAccountLegalEntityQuery> validator)
        {
            _providerService = providerService;
            _encodingService = encodingService;
            _validator = validator;
        }

        public async Task<GetAccountLegalEntityResult> Handle(GetAccountLegalEntityQuery query, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(query);

            if (!validationResult.IsValid())
            {
                throw new ValidationException(validationResult.ConvertToDataAnnotationsValidationResult(), null, null);
            }

            var legalEntityId = _encodingService.Decode(
                query.AccountLegalEntityPublicHashedId,
                EncodingType.PublicAccountLegalEntityId);

            var legalEntity = await _providerService.GetAccountLegalEntityById(legalEntityId);

            return new GetAccountLegalEntityResult
            {
                LegalEntity = legalEntity
            };
        }
    }
}
