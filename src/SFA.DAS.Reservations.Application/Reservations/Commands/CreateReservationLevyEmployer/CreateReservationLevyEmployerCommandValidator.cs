using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using SFA.DAS.Encoding;
using SFA.DAS.Reservations.Application.Validation;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Domain.Reservations.Api;
using SFA.DAS.Reservations.Infrastructure.Api;
using SFA.DAS.Reservations.Infrastructure.Configuration;

namespace SFA.DAS.Reservations.Application.Reservations.Commands.CreateReservationLevyEmployer
{
    public class CreateReservationLevyEmployerCommandValidator : IValidator<CreateReservationLevyEmployerCommand>
    {
        private readonly IEmployerAccountService _employerAccountService;
        private readonly IApiClient _apiClient;
        private readonly IReservationsService _reservationsService;
        private readonly IEncodingService _encodingService;
        private readonly ReservationsApiConfiguration _config;

        public CreateReservationLevyEmployerCommandValidator(IEmployerAccountService employerAccountService,
            IApiClient apiClient, IOptions<ReservationsApiConfiguration> config, IEncodingService encodingService, IReservationsService reservationsService)
        {
            _employerAccountService = employerAccountService;
            _apiClient = apiClient;
            _encodingService = encodingService;
            _reservationsService = reservationsService;
            _config = config.Value;
        }

        public async Task<ValidationResult> ValidateAsync(CreateReservationLevyEmployerCommand query)
        {
            var validationResult = new ValidationResult();
            
            if (query.AccountId < 1)
            {
                validationResult.AddError(nameof(query.AccountId));
            }

            if (query.AccountLegalEntityId < 1)
            {
                validationResult.AddError(nameof(query.AccountLegalEntityId));
            }

            if (!validationResult.IsValid())
            {
                return validationResult;
            }

            if (!string.IsNullOrEmpty(query.TransferSenderEmployerAccountId))
            {
                var pledgeApplicationId = string.IsNullOrWhiteSpace(query.EncodedPledgeApplicationId)
                    ? default(int?)
                    : (int) _encodingService.Decode(query.EncodedPledgeApplicationId, EncodingType.PledgeApplicationId);

                var transferValidationResult = await _reservationsService.GetTransferValidity(query.TransferSenderId.Value, query.AccountId, pledgeApplicationId);

                validationResult.FailedTransferReceiverCheck = !transferValidationResult.IsValid;
                return validationResult;
            }

            var response =
                await _apiClient.Get<AccountReservationStatusResponse>(
                    new AccountReservationStatusRequest(_config.Url, query.AccountId));

            validationResult.FailedAutoReservationCheck = !response.CanAutoCreateReservations;
            validationResult.FailedAgreementSignedCheck = !response.AccountLegalEntityAgreementStatus[query.AccountLegalEntityId];
            
            return validationResult;
        }
    }
}
