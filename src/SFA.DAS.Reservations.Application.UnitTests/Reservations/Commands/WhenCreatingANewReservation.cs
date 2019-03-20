using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.NUnit3;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.Reservations.Commands;
using SFA.DAS.Reservations.Application.Reservations.Queries;
using SFA.DAS.Reservations.Application.Reservations.Services;
using SFA.DAS.Reservations.Application.Validation;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Domain.Reservations.Api;
using SFA.DAS.Reservations.Infrastructure.Api;
using ValidationResult = SFA.DAS.Reservations.Application.Validation.ValidationResult;

namespace SFA.DAS.Reservations.Application.UnitTests.Reservations.Commands
{
    [TestFixture]
    public class WhenCreatingANewReservation
    {
        private Mock<IValidator<CreateReservationCommand>> _mockCreateCommandValidator;
        private Mock<IValidator<IReservationQuery>> _mockGetQueryValidator;
        private Mock<IApiClient> _mockApiClient;
        private CreateReservationCommandHandler _commandHandler;
        private CreateReservationResponse _apiResponse;
        private Mock<IHashingService> _mockHashingService;
        private Mock<ICacheStorageService> _mockCacheService;
        private long _expectedAccountId;
        private DateTime _expectedStartDate;
        private GetCachedReservationResult _cachedReservationResult;

        [SetUp]
        public void Arrange()
        {
            var fixture = new Fixture()
                .Customize(new AutoMoqCustomization{ConfigureMembers = true});

            _cachedReservationResult = fixture.Create<GetCachedReservationResult>();
            _expectedStartDate = fixture.Create<DateTime>().Date;
            _cachedReservationResult.StartDate = $"{_expectedStartDate:yyyy-MM}";
            _apiResponse = fixture.Create<CreateReservationResponse>();
            _expectedAccountId = fixture.Create<long>();

            _mockCreateCommandValidator = fixture.Freeze<Mock<IValidator<CreateReservationCommand>>>();
            _mockCreateCommandValidator
                .Setup(validator => validator.ValidateAsync(It.IsAny<CreateReservationCommand>()))
                .ReturnsAsync(new ValidationResult());

            _mockGetQueryValidator = fixture.Freeze<Mock<IValidator<IReservationQuery>>>();
            _mockGetQueryValidator
                .Setup(validator => validator.ValidateAsync(It.IsAny<IReservationQuery>()))
                .ReturnsAsync(new ValidationResult());

            _mockApiClient = fixture.Freeze<Mock<IApiClient>>();
            _mockApiClient
                .Setup(client => client.Create<ReservationApiRequest, CreateReservationResponse>(It.IsAny<ReservationApiRequest>()))
                .ReturnsAsync(_apiResponse);

            _mockHashingService = fixture.Freeze<Mock<IHashingService>>();
            _mockHashingService
                .Setup(service => service.DecodeValue(It.IsAny<string>()))
                .Returns(_expectedAccountId);

            _mockCacheService = fixture.Freeze<Mock<ICacheStorageService>>();
            _mockCacheService
                .Setup(service => service.RetrieveFromCache<GetCachedReservationResult>(It.IsAny<string>()))
                .ReturnsAsync(_cachedReservationResult);

            _commandHandler = fixture.Create<CreateReservationCommandHandler>();
        }

        [Test, AutoData]
        public async Task Then_It_Validates_The_Command(
            CreateReservationCommand command)
        {
            await _commandHandler.Handle(command, CancellationToken.None);

            _mockCreateCommandValidator.Verify(validator => validator.ValidateAsync(command), Times.Once);//todo: only needs to validate the guid
        }

        [Test, AutoData]
        public void And_The_Command_Is_Not_Valid_Then_Throws_ArgumentException(
            CreateReservationCommand command,
            ValidationResult validationResult,
            string propertyName)
        {
            validationResult.AddError(propertyName);

            _mockCreateCommandValidator
                .Setup(validator => validator.ValidateAsync(command))
                .ReturnsAsync(validationResult);

            Func<Task> act = async () => { await _commandHandler.Handle(command, CancellationToken.None); };

            act.Should().ThrowExactly<ValidationException>()
                .Which.ValidationResult.MemberNames.First(c=>c.StartsWith(propertyName)).Should().NotBeNullOrEmpty();
        }

        [Test, AutoData]
        public async Task Then_Gets_Reservation_From_The_Cache(
            CreateReservationCommand command)
        {
            await _commandHandler.Handle(command, CancellationToken.None);

            _mockCacheService.Verify(service => service.RetrieveFromCache<GetCachedReservationResult>(command.Id.ToString()));
        }

        [Test, AutoData]
        public async Task Then_It_Validates_The_Cached_Reservation(
            CreateReservationCommand command)
        {
            await _commandHandler.Handle(command, CancellationToken.None);

            _mockCreateCommandValidator.Verify(validator => validator.ValidateAsync(command), Times.Once);//todo: needs to be validating all properties
        }

        [Test, AutoData]
        public async Task Then_Calls_Reservation_Api_To_Create_Reservation_Without_Course(
            CreateReservationCommand command)
        {
            _cachedReservationResult.CourseId = null;

            await _commandHandler.Handle(command, CancellationToken.None);

            _mockApiClient.Verify(client => client.Create<ReservationApiRequest, CreateReservationResponse>(It.Is<ReservationApiRequest>(apiRequest => 
                apiRequest.AccountId == _expectedAccountId &&
                apiRequest.StartDate == $"{_expectedStartDate:yyyy-MMM}-01" &&
                apiRequest.CourseId == null)), Times.Once);
        }

        [Test, AutoData]
        public async Task Then_Calls_Reservation_Api_To_Create_Reservation_With_Course(
            CreateReservationCommand command)
        {
            _cachedReservationResult.CourseId = "123-1";

            await _commandHandler.Handle(command, CancellationToken.None);

            _mockApiClient.Verify(client => client.Create<ReservationApiRequest, CreateReservationResponse>(It.Is<ReservationApiRequest>(apiRequest => 
                    apiRequest.AccountId == _expectedAccountId &&
                    apiRequest.StartDate == $"{_expectedStartDate:yyyy-MMM}-01" &&
                    apiRequest.CourseId.Equals("123-1"))), Times.Once);
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

            result.Reservation.Id.Should().Be(_apiResponse.Id);
        }
    }
}