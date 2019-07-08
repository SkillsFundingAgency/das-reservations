using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Options;
using SFA.DAS.Reservations.Application.Exceptions;
using SFA.DAS.Reservations.Application.Validation;
using SFA.DAS.Reservations.Domain.Employers;
using SFA.DAS.Reservations.Domain.Employers.Api;
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
        private readonly AccountApiConfiguration _accountApiConfiguration;
        private readonly ReservationsApiConfiguration _config;


        public GetAccountReservationStatusQueryHandler(IApiClient apiClient,
            IValidator<GetAccountReservationStatusQuery> validator,
            IOptions<ReservationsApiConfiguration> configOptions, IOptions<AccountApiConfiguration> accountApiConfiguration)
        {
            _apiClient = apiClient;
            _validator = validator;
            _accountApiConfiguration = accountApiConfiguration.Value;
            _config = configOptions.Value;
        }

        public async Task<GetAccountReservationStatusResponse> Handle(GetAccountReservationStatusQuery request, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request);

            if (!validationResult.IsValid())
            {
                throw new ValidationException(
                    new ValidationResult("The following parameters have failed validation", validationResult.ErrorList), null, null);
            }


            if (!string.IsNullOrEmpty(request.TransferSenderAccountId) &&
                !string.IsNullOrEmpty(request.HashedEmployerAccountId))
            {
                var transferSenderResponse = await _apiClient.GetAll<EmployerTransferConnection>(
                    new GetEmployerTransferConnectionsRequest(_accountApiConfiguration.ApiBaseUrl,request.HashedEmployerAccountId));

                if (transferSenderResponse.ToList().Find(c =>
                        c.FundingEmployerPublicHashedAccountId.Equals(request.TransferSenderAccountId,
                            StringComparison.CurrentCultureIgnoreCase)) != null)
                {
                    return new GetAccountReservationStatusResponse{CanAutoCreateReservations = true};
                }

                throw new TransferSendNotAllowedException(request.AccountId, request.TransferSenderAccountId);

            }

            var response =
                await _apiClient.Get<AccountReservationStatusResponse>(
                    new AccountReservationStatusRequest(_config.Url, request.AccountId));

            return new GetAccountReservationStatusResponse{CanAutoCreateReservations = response.CanAutoCreateReservations};

        }
    }
}
