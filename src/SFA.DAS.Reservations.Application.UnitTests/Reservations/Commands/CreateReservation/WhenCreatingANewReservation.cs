using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.NUnit3;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.Reservations.Commands.CreateReservation;
using SFA.DAS.Reservations.Application.Validation;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Domain.Reservations;
using SFA.DAS.Reservations.Domain.Reservations.Api;
using SFA.DAS.Reservations.Domain.Rules;
using SFA.DAS.Reservations.Infrastructure.Api;
using SFA.DAS.Reservations.Infrastructure.Configuration;
using SFA.DAS.Reservations.Infrastructure.Exceptions;
using ValidationResult = SFA.DAS.Reservations.Application.Validation.ValidationResult;

namespace SFA.DAS.Reservations.Application.UnitTests.Reservations.Commands.CreateReservation
{
    [TestFixture]
    public class WhenCreatingANewReservation
    {
        private Mock<IValidator<CreateReservationCommand>> _mockCreateCommandValidator;
        private Mock<IValidator<CachedReservation>> _mockCachedReservationValidator;
        private Mock<IApiClient> _mockApiClient;
        private CreateReservationCommandHandler _commandHandler;
        private CreateReservationResponse _apiResponse;
        private Mock<ICacheStorageService> _mockCacheService;
        private Mock<ICachedReservationRespository> _mockCacheRepository;
        private DateTime _expectedStartDate;
        private CachedReservation _cachedReservation;
        private long _expectedAccountId = 12;
        private string _expectedLegalEntityName = "Test Entity";
        private Guid _expectedUserId;
        private IOptions<ReservationsApiConfiguration> _options;

        [SetUp]
        public void Arrange()
        {
            _expectedUserId = Guid.NewGuid();

            var fixture = new Fixture()
                .Customize(new AutoMoqCustomization{ConfigureMembers = true});

            _cachedReservation = fixture.Create<CachedReservation>();
            _expectedStartDate = fixture.Create<DateTime>().Date;
            _cachedReservation.TrainingDate = new TrainingDateModel{ StartDate = _expectedStartDate };
            _cachedReservation.AccountId = _expectedAccountId;
            _cachedReservation.AccountLegalEntityName = _expectedLegalEntityName;
            _cachedReservation.IsEmptyCohortFromSelect = false;
            _apiResponse = fixture.Create<CreateReservationResponse>();

            _mockCreateCommandValidator = fixture.Freeze<Mock<IValidator<CreateReservationCommand>>>();
            
            _mockCreateCommandValidator
                .Setup(validator => validator.ValidateAsync(It.IsAny<CreateReservationCommand>()))
                .ReturnsAsync(new ValidationResult());

            _mockCachedReservationValidator = fixture.Freeze<Mock<IValidator<CachedReservation>>>();
            _mockCachedReservationValidator
                .Setup(validator => validator.ValidateAsync(It.IsAny<CachedReservation>()))
                .ReturnsAsync(new ValidationResult());

            _options = fixture.Freeze<IOptions<ReservationsApiConfiguration>>();

            _mockApiClient = fixture.Freeze<Mock<IApiClient>>();
            _mockApiClient
                .Setup(client => client.Create<CreateReservationResponse>(It.IsAny<ReservationApiRequest>()))
                .ReturnsAsync(_apiResponse);

            _mockCacheService = fixture.Freeze<Mock<ICacheStorageService>>();

            _mockCacheRepository = fixture.Freeze<Mock<ICachedReservationRespository>>();
            _mockCacheRepository.Setup(r => r.GetProviderReservation(It.IsAny<Guid>(), It.IsAny<uint>()))
                .ReturnsAsync(_cachedReservation);

            _commandHandler = new CreateReservationCommandHandler(
                _mockCreateCommandValidator.Object,
                _mockCachedReservationValidator.Object,
                _options,
                _mockApiClient.Object,
                _mockCacheService.Object,
                _mockCacheRepository.Object);
        }

        [Test, AutoData]
        public async Task Then_It_Validates_The_Id(
            CreateReservationCommand command)
        {
            await _commandHandler.Handle(command, CancellationToken.None);

            _mockCreateCommandValidator.Verify(validator => validator.ValidateAsync(command), Times.Once);
        }

        [Test, AutoData]
        public async Task And_The_Command_Is_Not_Valid_Then_Throws_ValidationException(
            CreateReservationCommand command,
            ValidationResult validationResult,
            string propertyName)
        {
            validationResult.AddError(propertyName);

            _mockCachedReservationValidator
                .Setup(validator => validator.ValidateAsync(_cachedReservation))
                .ReturnsAsync(validationResult);

            Func<Task> act = async () => { await _commandHandler.Handle(command, CancellationToken.None); };

            (await act.Should().ThrowExactlyAsync<ValidationException>())
                .Which.ValidationResult.MemberNames.First(c => c.StartsWith(propertyName)).Should().NotBeNullOrEmpty();
        }

        [Test, AutoData]
        public async Task Then_Gets_Provider_Reservation_From_The_Cache(CreateReservationCommand command)
        {
            await _commandHandler.Handle(command, CancellationToken.None);

            _mockCacheRepository.Verify(service => service.GetProviderReservation(command.Id, command.UkPrn.Value), Times.Once);
            _mockCacheRepository.Verify(service => service.GetEmployerReservation(It.IsAny<Guid>()), Times.Never);
        }

        [Test, AutoData]
        public async Task Then_Gets_Employer_Reservation_From_The_Cache(CreateReservationCommand command)
        {
            command.UkPrn = null;

            _mockCacheRepository.Setup(r => r.GetEmployerReservation(It.IsAny<Guid>()))
                .ReturnsAsync(_cachedReservation);

            await _commandHandler.Handle(command, CancellationToken.None);

            _mockCacheRepository.Verify(service => service.GetEmployerReservation(command.Id), Times.Once);
            _mockCacheRepository.Verify(service => service.GetProviderReservation(It.IsAny<Guid>(), It.IsAny<uint>()), Times.Never);
        }

        [Test, AutoData]
        public async Task And_No_Reservation_Found_In_Cache_Then_Throws_Exception(
            CreateReservationCommand command)
        {
            _mockCacheRepository
                .Setup(service => service.GetProviderReservation(It.IsAny<Guid>(), It.IsAny<uint>()))
                .ReturnsAsync((CachedReservation)null);
            _mockCacheRepository
                .Setup(service => service.GetEmployerReservation(It.IsAny<Guid>()))
                .ReturnsAsync((CachedReservation)null);

            Func<Task> act = async () => { await _commandHandler.Handle(command, CancellationToken.None); };

            (await act.Should().ThrowExactlyAsync<CachedReservationNotFoundException>())
                .WithMessage($"No reservation was found with id [{command.Id}].");
        }

        [Test, AutoData]
        public async Task Then_It_Validates_The_Cached_Reservation(
            CreateReservationCommand command)
        {
            await _commandHandler.Handle(command, CancellationToken.None);

            _mockCachedReservationValidator.Verify(validator => validator.ValidateAsync(_cachedReservation), Times.Once);
        }
        
        [Test, AutoData]
        public async Task Then_Calls_Reservation_Api_To_Create_Reservation_With_Course_And_UserId(
            CreateReservationCommand command)
        {
            _cachedReservation.CourseId = "123-1";
            command.UserId = _expectedUserId;

            await _commandHandler.Handle(command, CancellationToken.None);

            _mockApiClient.Verify(client => client.Create<CreateReservationResponse>(It.Is<ReservationApiRequest>(apiRequest => 
                    apiRequest.AccountId == _expectedAccountId &&
                    apiRequest.StartDate == $"{_expectedStartDate:yyyy-MMM}-01" &&
                    apiRequest.AccountLegalEntityName == _expectedLegalEntityName &&
                    apiRequest.UserId == _expectedUserId &&
                    apiRequest.CourseId.Equals("123-1"))), Times.Once);
        }

        [Test, AutoData]
        public async Task Then_The_Provider_Id_Is_Not_Stored_And_Passed_Back_Through_Command_If_Reservation_Is_Empty_Cohort(CreateReservationCommand command)
        {
            //Arrange
            _cachedReservation.IsEmptyCohortFromSelect = true;
            _cachedReservation.UkPrn = 12354;
            command.UserId = _expectedUserId;

            //Act
            var result = await _commandHandler.Handle(command, CancellationToken.None);

            //Assert
            _mockApiClient.Verify(client => client.Create<CreateReservationResponse>(It.Is<ReservationApiRequest>(apiRequest =>
                apiRequest.AccountId == _expectedAccountId &&
                apiRequest.ProviderId == null &&
                apiRequest.StartDate == $"{_expectedStartDate:yyyy-MMM}-01" &&
                apiRequest.AccountLegalEntityName == _expectedLegalEntityName &&
                apiRequest.UserId == _expectedUserId)), Times.Once);

            result.Id.Should().Be(_apiResponse.Id);
            result.AccountLegalEntityPublicHashedId.Should()
                .Be(_cachedReservation.AccountLegalEntityPublicHashedId);
            result.CohortRef.Should().Be(_cachedReservation.CohortRef);
            result.IsEmptyCohortFromSelect.Should().Be(_cachedReservation.IsEmptyCohortFromSelect);
            result.ProviderId.Should().Be(_cachedReservation.UkPrn);
        }

        [Test, AutoData]
        public async Task Then_Cache_Service_Removes_From_Cache(
            CreateReservationCommand command)
        {
            await _commandHandler.Handle(command, CancellationToken.None);

            _mockCacheService.Verify(service => service.DeleteFromCache(command.Id.ToString()), Times.Once);
        }

        [Test, AutoData]
        public async Task Then_Returns_Response_From_Reservation_Api(
            CreateReservationCommand command)
        {
            var result  = await _commandHandler.Handle(command, CancellationToken.None);

            result.Id.Should().Be(_apiResponse.Id);
            result.AccountLegalEntityPublicHashedId.Should()
                .Be(_cachedReservation.AccountLegalEntityPublicHashedId);
            result.CohortRef.Should().Be(_cachedReservation.CohortRef);
            result.JourneyData.Should().Be(_cachedReservation.JourneyData);
        }

    }
}