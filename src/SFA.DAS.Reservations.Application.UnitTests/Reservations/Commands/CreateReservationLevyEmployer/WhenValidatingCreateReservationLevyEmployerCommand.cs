using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using SFA.DAS.Encoding;
using SFA.DAS.Reservations.Application.Reservations.Commands.CreateReservationLevyEmployer;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Domain.Reservations;
using SFA.DAS.Reservations.Domain.Reservations.Api;
using SFA.DAS.Reservations.Infrastructure.Api;
using SFA.DAS.Reservations.Infrastructure.Configuration;

namespace SFA.DAS.Reservations.Application.UnitTests.Reservations.Commands.CreateReservationLevyEmployer
{
    [TestFixture]
    public class WhenValidatingCreateReservationLevyEmployerCommand
    {
        private CreateReservationLevyEmployerCommandValidator _validator;
        private Mock<IApiClient> _apiClient;
        private Mock<IOptions<ReservationsApiConfiguration>> _config;
        private Mock<IEncodingService> _encodingService;
        private Mock<IReservationsOuterService> _reservationsService;
        private const string ExpectedTransferSenderEmployerAccountId = "TGB456";
        private const long ExpectedAccountId = 432;
        private const long ExpectedAccountLegalEntityId = 9895;
        private const string ExpectedAccountHashedId = "CSQ212K";
        private const string ExpectedUrl = "https://test.local";
        private GetTransferValidityResponse TransferValidityResponse;

        [SetUp]
        public void Arrange()
        {
            TransferValidityResponse = new GetTransferValidityResponse{ IsValid = true };

            _reservationsService = new Mock<IReservationsOuterService>();
            _reservationsService.Setup(x =>
                    x.GetTransferValidity(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<int?>()))
                .ReturnsAsync(TransferValidityResponse);

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

            var config = new ReservationsApiConfiguration
            {
                Url = ExpectedUrl
            };

            _config = new Mock<IOptions<ReservationsApiConfiguration>>();
            _config.Setup(opt => opt.Value).Returns(config);

            _encodingService = new Mock<IEncodingService>();
            _encodingService.Setup(x => x.Encode(ExpectedAccountId, EncodingType.AccountId)).Returns(ExpectedAccountHashedId);

            _validator = new CreateReservationLevyEmployerCommandValidator(_apiClient.Object, _config.Object, _encodingService.Object, _reservationsService.Object);
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
            result.IsValid().Should().BeFalse();
            result.ValidationDictionary.ContainsKey(nameof(command.AccountId)).Should().BeTrue();
            result.ValidationDictionary.ContainsKey(nameof(command.AccountLegalEntityId)).Should().BeTrue();

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
            result.IsValid().Should();

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
            result.IsValid().Should().BeFalse();
            result.ValidationDictionary.ContainsKey(nameof(command.AccountId)).Should().BeTrue();
            result.ValidationDictionary.ContainsKey(nameof(command.AccountLegalEntityId)).Should().BeFalse();

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
            result.IsValid().Should();
            result.ValidationDictionary.ContainsKey(nameof(command.AccountId)).Should().BeFalse();
            result.ValidationDictionary.ContainsKey(nameof(command.AccountLegalEntityId)).Should().BeTrue();
        }

        [TestCase(null)]
        [TestCase(789)]
        public async Task Then_If_There_Is_A_Transfer_Sender_Then_Transfer_Is_Validated(int? pledgeApplicationId)
        {
            //Arrange
            TransferValidityResponse.IsValid = false;

            if (pledgeApplicationId.HasValue)
            {
                _encodingService.Setup(x => x.Decode("EncodedPledgeApplicationId", EncodingType.PledgeApplicationId))
                    .Returns(pledgeApplicationId.Value);
            }

            var command = new CreateReservationLevyEmployerCommand
            {
                AccountId = ExpectedAccountId,
                AccountLegalEntityId = 234,
                TransferSenderId = 345,
                TransferSenderEmployerAccountId = ExpectedTransferSenderEmployerAccountId,
                EncodedPledgeApplicationId = pledgeApplicationId.HasValue ? "EncodedPledgeApplicationId" : string.Empty
            };

            //Act
            await _validator.ValidateAsync(command);

            //Assert
            _reservationsService.Verify(x => x.GetTransferValidity(It.Is<long>(s => s == 345), It.Is<long>(r => r == ExpectedAccountId), It.Is<int?>(a => a == pledgeApplicationId)));
        }

        [Test]
        public async Task Then_If_There_Is_A_Transfer_Sender_Then_Validation_Fails_If_Transfer_Is_Invalid()
        {
            //Arrange
            TransferValidityResponse.IsValid = false;

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
            result.FailedTransferReceiverCheck.Should().BeTrue();
        }

        [Test]
        public async Task Then_If_There_Is_A_Transfer_Sender_Then_Validation_Succeeds_If_Transfer_Is_Valid()
        {
            //Arrange
            TransferValidityResponse.IsValid = true;

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
            result.FailedTransferReceiverCheck.Should().BeFalse();
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

            result.FailedAutoReservationCheck.Should().BeFalse();
            result.FailedAgreementSignedCheck.Should().BeTrue();
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
            result.FailedAutoReservationCheck.Should().BeTrue();
        }
    }
}
