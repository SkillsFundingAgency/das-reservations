using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Options;
using SFA.DAS.Reservations.Application.Exceptions;
using SFA.DAS.Reservations.Application.Extensions;
using SFA.DAS.Reservations.Application.Validation;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Domain.Reservations.Api;
using SFA.DAS.Reservations.Infrastructure.Api;
using SFA.DAS.Reservations.Infrastructure.Configuration;
using ValidationResult = System.ComponentModel.DataAnnotations.ValidationResult;

namespace SFA.DAS.Reservations.Application.Reservations.Queries.GetAccountReservationStatus
{
    public class GetAccountReservationStatusQueryHandler : IRequestHandler<GetAccountReservationStatusQuery, GetAccountReservationStatusResponse>
    {
        private readonly IApiClient _apiClient;
        private readonly IValidator<GetAccountReservationStatusQuery> _validator;
        private readonly IEmployerAccountService _accountsService;
        private readonly ReservationsApiConfiguration _config;


        public GetAccountReservationStatusQueryHandler(IApiClient apiClient,
            IValidator<GetAccountReservationStatusQuery> validator,
            IOptions<ReservationsApiConfiguration> configOptions, IEmployerAccountService accountsService)
        {
            _apiClient = apiClient;
            _validator = validator;
            _accountsService = accountsService;
            _config = configOptions.Value;
        }

        public async Task<GetAccountReservationStatusResponse> Handle(GetAccountReservationStatusQuery request, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request);

            if (!validationResult.IsValid())
            {
                throw new ValidationException(validationResult.ConvertToDataAnnotationsValidationResult(), null, null);
            }


            if (!string.IsNullOrEmpty(request.TransferSenderAccountId) &&
                !string.IsNullOrEmpty(request.HashedEmployerAccountId))
            {
                var transferSenderResponse = await _accountsService.GetTransferConnections(request.HashedEmployerAccountId);

                var employerTransferConnection = transferSenderResponse.ToList().Find(c =>
                    c.FundingEmployerPublicHashedAccountId.Equals(request.TransferSenderAccountId,
                        StringComparison.CurrentCultureIgnoreCase));
                if (employerTransferConnection != null)
                {
                    return new GetAccountReservationStatusResponse
                    {
                        CanAutoCreateReservations = true,
                        TransferAccountId = employerTransferConnection.FundingEmployerAccountId

                    };
                }

                throw new TransferSenderNotAllowedException(request.AccountId, request.TransferSenderAccountId);

            }

            var response =
                await _apiClient.Get<AccountReservationStatusResponse>(
                    new AccountReservationStatusRequest(_config.Url, request.AccountId));

            return new GetAccountReservationStatusResponse{CanAutoCreateReservations = response.CanAutoCreateReservations};

        }
    }
}
