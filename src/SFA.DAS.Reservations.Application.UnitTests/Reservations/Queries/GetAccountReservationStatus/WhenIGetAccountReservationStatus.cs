using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.Exceptions;
using SFA.DAS.Reservations.Application.Reservations.Queries.GetAccountReservationStatus;
using SFA.DAS.Reservations.Application.Validation;
using SFA.DAS.Reservations.Domain.Employers;
using SFA.DAS.Reservations.Domain.Employers.Api;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Domain.Reservations.Api;
using SFA.DAS.Reservations.Infrastructure.Api;
using SFA.DAS.Reservations.Infrastructure.Configuration;
using ValidationResult = SFA.DAS.Reservations.Application.Validation.ValidationResult;

namespace SFA.DAS.Reservations.Application.UnitTests.Reservations.Queries.GetAccountReservationStatus
{
    [TestFixture]
    public class WhenIGetAccountReservationStatus
    {
        private Mock<IApiClient> _apiClient;
        private GetAccountReservationStatusQueryHandler _handler;
        private Mock<IValidator<GetAccountReservationStatusQuery>> _validator;
        private Mock<IOptions<ReservationsApiConfiguration>> _configOptions;
        private Mock<IEmployerAccountService> _employerAccountService;


        [SetUp]
        public void SetUp()
        {
            _apiClient = new Mock<IApiClient>();
            _configOptions = new Mock<IOptions<ReservationsApiConfiguration>>();
            _configOptions.Setup(x => x.Value.Url).Returns("test/test");

            _employerAccountService = new Mock<IEmployerAccountService>();

            _validator = new Mock<IValidator<GetAccountReservationStatusQuery>>();
            _validator
                .Setup(x => x.ValidateAsync(It.IsAny<GetAccountReservationStatusQuery>()))
                .ReturnsAsync(new ValidationResult { ValidationDictionary = new Dictionary<string, string>() });
            _handler = new GetAccountReservationStatusQueryHandler(_apiClient.Object, _validator.Object,_configOptions.Object, _employerAccountService.Object);
        }

        [Test]
        public async Task AndTheQueryIsValid_ThenTheReservationStatusIsReturned()
        {
            //Arrange
            var apiResponse = new AccountReservationStatusResponse
            {
                CanAutoCreateReservations = true
            };
            _apiClient
                .Setup(x => x.Get<AccountReservationStatusResponse>(It.IsAny<AccountReservationStatusRequest>()))
                .ReturnsAsync(apiResponse);
            var query = new GetAccountReservationStatusQuery {AccountId = 123456};
            

            //Act
            var result = await _handler.Handle(query, CancellationToken.None);

            //Assert
            Assert.AreEqual(apiResponse.CanAutoCreateReservations, result.CanAutoCreateReservations);
            Assert.AreEqual(0, result.TransferAccountId);
            _apiClient.Verify(x => x.GetAll<EmployerTransferConnection>(It.IsAny<GetEmployerTransferConnectionsRequest>()), Times.Never);
        }

        [Test]
        public void Then_If_The_TransferSenderId_Is_Included_Then_It_Is_Checked_Against_The_Valid_Senders_And_If_Not_Matched_An_Error_Is_Thrown()
        {
            //Arrange
            var query = new GetAccountReservationStatusQuery { AccountId = 123456, HashedEmployerAccountId = "TGB32", TransferSenderAccountId = "423EDC" };
            
            _employerAccountService.Setup(x =>
                x.GetTransferConnections(query.HashedEmployerAccountId))
                .ReturnsAsync(new List<EmployerTransferConnection>{new EmployerTransferConnection
                {
                    FundingEmployerAccountId = 1,
                    FundingEmployerAccountName = "Test",
                    FundingEmployerHashedAccountId = "123EDC",
                    FundingEmployerPublicHashedAccountId = "YTR34"
                }});

            //Act Assert
            Assert.ThrowsAsync<TransferSenderNotAllowedException>(() => _handler.Handle(query, CancellationToken.None));
        }

        [Test]
        public async Task Then_If_The_TransferSenderId_Is_Included_And_An_Allowed_Connection_And_The_Employer_Reservation_Status_Is_Not_AutoCreate_A_Can_Auto_Create_Reservation_Status_Is_Returned()
        {
            //Arrange
            var expectedTransferAccountId = 10;
            var query = new GetAccountReservationStatusQuery { AccountId = 123456, HashedEmployerAccountId = "TGB32", TransferSenderAccountId = "423EDC" };
            var apiResponse = new AccountReservationStatusResponse
            {
                CanAutoCreateReservations = false
            };
            _apiClient
                .Setup(x => x.Get<AccountReservationStatusResponse>(It.Is<AccountReservationStatusRequest>(c=>c.AccountId.Equals(query.AccountId))))
                .ReturnsAsync(apiResponse);
            _employerAccountService.Setup(x =>
                    x.GetTransferConnections(query.HashedEmployerAccountId))
                .ReturnsAsync(new List<EmployerTransferConnection>{new EmployerTransferConnection
                {
                    FundingEmployerAccountId = expectedTransferAccountId,
                    FundingEmployerAccountName = "Test",
                    FundingEmployerHashedAccountId = "123EDC",
                    FundingEmployerPublicHashedAccountId = "423EDC"
                }});

            //Act
            var result = await _handler.Handle(query, CancellationToken.None);

            //Assert
            Assert.AreEqual(expectedTransferAccountId,result.TransferAccountId);
            Assert.IsTrue(result.CanAutoCreateReservations);
            _apiClient.Verify(x => x.Get<AccountReservationStatusResponse>(It.IsAny<AccountReservationStatusRequest>()), Times.Never);
        }

        [Test]
        public void AndTheQueryIsNotValid_ThenValidationErrorThrown()
        {
            //Arrange
            _apiClient
                .Setup(x => x.Get<AccountReservationStatusResponse>(It.IsAny<AccountReservationStatusRequest>()))
                .ReturnsAsync(new AccountReservationStatusResponse());
            var query = new GetAccountReservationStatusQuery {AccountId = default(long)};
            _validator
                .Setup(x => x.ValidateAsync(It.IsAny<GetAccountReservationStatusQuery>()))
                .ReturnsAsync(new ValidationResult
                {
                    ValidationDictionary = new Dictionary<string, string>()
                    {
                        {"test", "test132"}
                    }
                });

            //Act + Assert
            Assert.ThrowsAsync<ValidationException>(() =>
                _handler.Handle(query, CancellationToken.None)
            );
        }
    }
}