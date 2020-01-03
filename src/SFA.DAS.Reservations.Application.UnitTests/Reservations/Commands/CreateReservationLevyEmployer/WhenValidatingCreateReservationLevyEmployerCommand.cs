using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using SFA.DAS.Encoding;
using SFA.DAS.Reservations.Application.Reservations.Commands.CreateReservationLevyEmployer;
using SFA.DAS.Reservations.Domain.Employers;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Domain.Reservations.Api;
using SFA.DAS.Reservations.Infrastructure.Api;
using SFA.DAS.Reservations.Infrastructure.Configuration;

namespace SFA.DAS.Reservations.Application.UnitTests.Reservations.Commands.CreateReservationLevyEmployer
{
    [TestFixture]
    public class WhenValidatingCreateReservationLevyEmployerCommand
    {
        private CreateReservationLevyEmployerCommandValidator _validator;
        private Mock<IEmployerAccountService> _employerAccountService;
        private Mock<IApiClient> _apiClient;
        private Mock<IOptions<ReservationsApiConfiguration>> _config;
        private Mock<IEncodingService> _encodingService;
        private const string ExpectedTransferSenderEmployerAccountId = "TGB456";
        private const long ExpectedAccountId = 432;
        private const long ExpectedAccountLegalEntityId = 9895;
        private const string ExpectedAccountHashedId = "CSQ212K";
        private const string ExpectedUrl = "https://test.local";

        [SetUp]
        public void Arrange()
        {
            _employerAccountService = new Mock<IEmployerAccountService>();
            _apiClient = new Mock<IApiClient>();
            _apiClient.Setup(x => x.Get<AccountReservationStatusResponse>(It.IsAny<AccountReservationStatusRequest>()))
                .ReturnsAsync(new AccountReservationStatusResponse
                {
                    CanAutoCreateReservations = false,
                    AccountLegalEntityAgreementStatus = new Dictionary<long, bool>{
                    {
                        ExpectedAccountLegalEntityId,false
                    }}
                });
            _apiClient.Setup(x => x.Get<AccountReservationStatusResponse>
                (It.Is<AccountReservationStatusRequest>(c =>
                    c.BaseUrl.Equals(ExpectedUrl) && c.AccountId.Equals(ExpectedAccountId))))
                .ReturnsAsync(new AccountReservationStatusResponse
                {
                    CanAutoCreateReservations = true,
                    AccountLegalEntityAgreementStatus = new Dictionary<long, bool>{
                    {
                        ExpectedAccountLegalEntityId,false
                    }}
                });
            _config = new Mock<IOptions<ReservationsApiConfiguration>>();
            _config.Setup(x => x.Value.Url).Returns(ExpectedUrl);

            _encodingService = new Mock<IEncodingService>();
            _encodingService.Setup(x => x.Encode(ExpectedAccountId, EncodingType.AccountId)).Returns(ExpectedAccountHashedId);

            _validator = new CreateReservationLevyEmployerCommandValidator(_employerAccountService.Object, _apiClient.Object, _config.Object, _encodingService.Object);
        }

        [Test]
        public async Task AndTheCommandIsInvalid_ThenAddsError()
        {
            //Arrange
            var command = new CreateReservationLevyEmployerCommand
            {
                AccountId = -234,
                AccountLegalEntityId = -3
            };

            //Act
            var result = await _validator.ValidateAsync(command);

            //Assert
            Assert.False(result.IsValid());
            Assert.True(result.ValidationDictionary.ContainsKey(nameof(command.AccountId)));
            Assert.True(result.ValidationDictionary.ContainsKey(nameof(command.AccountLegalEntityId)));

        }
        [Test]
        public async Task AndTheCommandIsValid_ThenNoErrorsAdded()
        {
            //Arrange
            var command = new CreateReservationLevyEmployerCommand
            {
                AccountId = ExpectedAccountId,
                AccountLegalEntityId = ExpectedAccountLegalEntityId
            };

            //Act
            var result = await _validator.ValidateAsync(command);

            //Assert
            Assert.True(result.IsValid());

        }
        [Test]
        public async Task AndOnlyAccountIdInvalid_ThenAddsError()
        {
            //Arrange
            var command = new CreateReservationLevyEmployerCommand
            {
                AccountId = -234,
                AccountLegalEntityId = 5131
            };

            //Act
            var result = await _validator.ValidateAsync(command);

            //Assert
            Assert.False(result.IsValid());
            Assert.True(result.ValidationDictionary.ContainsKey(nameof(command.AccountId)));
            Assert.False(result.ValidationDictionary.ContainsKey(nameof(command.AccountLegalEntityId)));

        }
        [Test]
        public async Task AndOnlyAccountLegalEntityIdInvalid_ThenAddsError()
        {
            //Arrange
            var command = new CreateReservationLevyEmployerCommand
            {
                AccountId = ExpectedAccountId,
                AccountLegalEntityId = -234
            };

            //Act
            var result = await _validator.ValidateAsync(command);

            //Assert
            Assert.False(result.IsValid());
            Assert.False(result.ValidationDictionary.ContainsKey(nameof(command.AccountId)));
            Assert.True(result.ValidationDictionary.ContainsKey(nameof(command.AccountLegalEntityId)));
        }

        [Test]
        public async Task Then_If_There_Is_A_Transfer_Id_It_Is_Checked_To_See_If_It_Is_A_Valid_Receiver_And_The_Api_Is_Not_Checked_And_FailedAutoReservationCheck_Is_False()
        {
            //Arrange
            _employerAccountService.Setup(x =>
                    x.GetTransferConnections(ExpectedAccountHashedId))
                .ReturnsAsync(new List<EmployerTransferConnection>{new EmployerTransferConnection
                {
                    FundingEmployerAccountId = 78954,
                    FundingEmployerAccountName = "Test",
                    FundingEmployerHashedAccountId = "123EDC",
                    FundingEmployerPublicHashedAccountId = ExpectedTransferSenderEmployerAccountId
                }});
            var command = new CreateReservationLevyEmployerCommand
            {
                AccountId = ExpectedAccountId,
                AccountLegalEntityId = 234,
                TransferSenderId = 345,
                TransferSenderEmployerAccountId = ExpectedTransferSenderEmployerAccountId
            };

            //Act
            var result = await _validator.ValidateAsync(command);

            //Assert
            _employerAccountService.Verify(x=>x.GetTransferConnections(ExpectedAccountHashedId));
            Assert.IsFalse(result.FailedTransferReceiverCheck);
            _apiClient.Verify(x=> x.Get<AccountReservationStatusResponse>(It.IsAny<AccountReservationStatusRequest>()), Times.Never);
            Assert.IsFalse(result.FailedAutoReservationCheck);
        }

        [Test]
        public async Task Then_If_It_Is_Not_A_Valid_Receiver_The_FailedTransferReceiver_Flag_Is_Set()
        {
            //Arrange
            _employerAccountService.Setup(x =>
                    x.GetTransferConnections(ExpectedTransferSenderEmployerAccountId))
                .ReturnsAsync(new List<EmployerTransferConnection>{new EmployerTransferConnection
                {
                    FundingEmployerAccountId = 1,
                    FundingEmployerAccountName = "Test",
                    FundingEmployerHashedAccountId = "123EDC",
                    FundingEmployerPublicHashedAccountId = "YTR34"
                }});
            var command = new CreateReservationLevyEmployerCommand
            {
                TransferSenderEmployerAccountId = ExpectedTransferSenderEmployerAccountId,
                AccountId = ExpectedAccountId,
                AccountLegalEntityId = 234,
                TransferSenderId = 245
            };

            //Act
            var result = await _validator.ValidateAsync(command);

            //Assert
            Assert.IsTrue(result.FailedTransferReceiverCheck);
        }

        [Test]
        public async Task Then_The_AccountId_Is_Checked_If_There_Is_No_TransferId_To_Make_Sure_It_Is_Able_To_Create_Levy_Reservations_And_Agreement_Status()
        {
            //Arrange
            var command = new CreateReservationLevyEmployerCommand
            {
                AccountId = ExpectedAccountId,
                AccountLegalEntityId = ExpectedAccountLegalEntityId
            };

            //Act
            var result = await _validator.ValidateAsync(command);

            //Assert
            _apiClient.Verify(x=>x.Get<AccountReservationStatusResponse>(
                It.Is<AccountReservationStatusRequest>(c=>c.BaseUrl.Equals(ExpectedUrl) 
                 && c.AccountId.Equals(ExpectedAccountId))), Times.Once);
            Assert.IsFalse(result.FailedAutoReservationCheck);
            Assert.IsTrue(result.FailedAgreementSignedCheck);
        }

        [Test]
        public async Task Then_The_AccountId_Is_Unable_To_Create_Auto_Reservations_A_Flag_Is_Set()
        {
            //Arrange
            var command = new CreateReservationLevyEmployerCommand
            {
                AccountId = 123,
                AccountLegalEntityId = ExpectedAccountLegalEntityId
            };

            //Act
            var result = await _validator.ValidateAsync(command);

            //Assert
            Assert.IsTrue(result.FailedAutoReservationCheck);
        }
    }
}
