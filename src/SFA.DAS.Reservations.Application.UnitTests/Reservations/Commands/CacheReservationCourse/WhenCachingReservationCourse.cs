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
using SFA.DAS.Reservations.Application.Exceptions;
using SFA.DAS.Reservations.Application.Reservations.Commands.CacheReservationCourse;
using SFA.DAS.Reservations.Application.Reservations.Queries.GetCourses;
using SFA.DAS.Reservations.Application.Reservations.Services;
using SFA.DAS.Reservations.Application.Validation;
using SFA.DAS.Reservations.Domain.Courses;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Domain.Reservations;
using ValidationResult = SFA.DAS.Reservations.Application.Validation.ValidationResult;

namespace SFA.DAS.Reservations.Application.UnitTests.Reservations.Commands.CacheReservationCourse
{
    public class WhenCachingReservationCourse
    {
        private Mock<IValidator<CacheReservationCourseCommand>> _mockValidator;
        private Mock<ICacheStorageService> _mockCacheStorageService;
        private Mock<ICourseService> _mockCourseService;
        private CacheReservationCourseCommandHandler _commandHandler;
        private Course _expectedCourse;
        private CachedReservation _cachedReservation;

        [SetUp]
        public void Arrange()
        {
            var fixture = new Fixture()
                .Customize(new AutoMoqCustomization{ConfigureMembers = true});

            _expectedCourse = fixture.Freeze<Course>();

            _mockValidator = fixture.Freeze<Mock<IValidator<CacheReservationCourseCommand>>>();
           
            _mockValidator
                .Setup(validator => validator.ValidateAsync(It.IsAny<CacheReservationCourseCommand>()))
                .ReturnsAsync(new ValidationResult());

            _cachedReservation = fixture.Freeze<CachedReservation>();

            _mockCacheStorageService = fixture.Freeze<Mock<ICacheStorageService>>();
            _mockCacheStorageService.Setup(s => s.RetrieveFromCache<CachedReservation>(It.IsAny<string>()))
                .ReturnsAsync(_cachedReservation);

            _mockCourseService = fixture.Freeze<Mock<ICourseService>>();
            _mockCourseService.Setup(cs => cs.GetCourse(It.IsAny<string>()))
                              .ReturnsAsync(_expectedCourse);

            _commandHandler = fixture.Create<CacheReservationCourseCommandHandler>();
        }

        [Test, AutoData]
        public async Task Then_It_Validates_The_Command(CacheReservationCourseCommand command)
        {
            //Act
            await _commandHandler.Handle(command, CancellationToken.None);

            //Assert
            _mockValidator.Verify(validator => validator.ValidateAsync(command), Times.Once);
        }

        [Test, AutoData]
        public void And_The_Command_Is_Not_Valid_Then_Throws_ArgumentException(
            CacheReservationCourseCommand command,
            ValidationResult validationResult,
            string propertyName)
        {
            //Assign
            validationResult.AddError(propertyName);

            _mockValidator
                .Setup(validator => validator.ValidateAsync(command))
                .ReturnsAsync(validationResult);

            //Act
            Func<Task> act = async () => { await _commandHandler.Handle(command, CancellationToken.None); };

            //Assert
            act.Should().ThrowExactly<ValidationException>()
                .Which.ValidationResult.MemberNames.First(c=>c.StartsWith(propertyName)).Should().NotBeNullOrEmpty();
        }

        
        [Test, AutoData]
        public void And_The_Command_Is_Not_Valid_Then_Does_Not_Get_Cached_Reservation(
            CacheReservationCourseCommand command,
            ValidationResult validationResult,
            string propertyName)
        {
            //Assign
            validationResult.AddError(propertyName);

            _mockValidator
                .Setup(validator => validator.ValidateAsync(command))
                .ReturnsAsync(validationResult);

            //Act
            Assert.ThrowsAsync<ValidationException>(() => _commandHandler.Handle(command, CancellationToken.None));

            //Assert
            _mockCacheStorageService.Verify(s => s.RetrieveFromCache<GetCoursesResult>(It.IsAny<string>()), Times.Never);
        }

        [Test, AutoData]
        public void And_The_Command_Is_Not_Valid_Then_Does_Not_Cache_Reservation(
            CacheReservationCourseCommand command,
            ValidationResult validationResult,
            string propertyName)
        {
            //Assign
            validationResult.AddError(propertyName);

            _mockValidator
                .Setup(validator => validator.ValidateAsync(command))
                .ReturnsAsync(validationResult);
            
            //Act
             Assert.ThrowsAsync<ValidationException>(() => _commandHandler.Handle(command, CancellationToken.None));

            //Assert
            _mockCacheStorageService.Verify(s => s.SaveToCache(It.IsAny<string>(), It.IsAny<CachedReservation>(), It.IsAny<int>()), Times.Never);
        }

        [Test, AutoData]
        public async Task Then_Gets_Cached_Reservation(CacheReservationCourseCommand command)
        {
            //Act
            await _commandHandler.Handle(command, CancellationToken.None);

            //Assert
            _mockCacheStorageService.Verify(service => service.RetrieveFromCache<CachedReservation>(command.Id.ToString()), Times.Once);
        }

        
        [Test, AutoData]
        public async Task Then_Gets_Selected_Course(CacheReservationCourseCommand command)
        {
            //Act
            await _commandHandler.Handle(command, CancellationToken.None);

            //Assert
            _mockCourseService.Verify(cs => cs.GetCourse(command.CourseId), Times.Once);
        }

        [Test, AutoData]
        public async Task Then_Calls_Cache_Service_To_Save_Reservation(
            CacheReservationCourseCommand command, 
            CachedReservation cachedReservation)
        {
            //Assign
            command.CourseId = _expectedCourse.Id;
            
            _mockCacheStorageService.Setup(s => s.RetrieveFromCache<CachedReservation>(It.IsAny<string>()))
                                    .ReturnsAsync(cachedReservation);

            //Act
            await _commandHandler.Handle(command, CancellationToken.None);

            //Assert
            _mockCacheStorageService.Verify(service => service.SaveToCache(
                It.IsAny<string>(), 
                It.Is<CachedReservation>(c => c.CourseId.Equals(_expectedCourse.Id) &&
                    c.CourseDescription.Equals(_expectedCourse.CourseDescription)), 
                1));
        }

        [Test, AutoData]
        public async Task Then_Caches_Course_Choice_If_Not_Selected(
            CacheReservationCourseCommand command, 
            CachedReservation cachedReservation)
        {
            //Assign
            command.CourseId = null;
            
            _mockCacheStorageService.Setup(s => s.RetrieveFromCache<CachedReservation>(It.IsAny<string>()))
                .ReturnsAsync(cachedReservation);

            //Act
            await _commandHandler.Handle(command, CancellationToken.None);

            //Assert
            _mockCourseService.Verify(s => s.GetCourse(It.IsAny<string>()), Times.Never);
            _mockCacheStorageService.Verify(service => service.SaveToCache(
                It.IsAny<string>(), 
                It.Is<CachedReservation>(c => c.CourseId == null && c.CourseDescription == null), 
                1));
        }

        [Test, AutoData]
        public void Then_Throws_Exception_If_Reservation_Not_Found_In_Cache(CacheReservationCourseCommand command)
        {
            //Assign
            _mockCacheStorageService.Setup(s => s.RetrieveFromCache<CachedReservation>(It.IsAny<string>()))
                .ReturnsAsync((CachedReservation) null);

            //Act
            Assert.ThrowsAsync<CachedReservationNotFoundException>(() => _commandHandler.Handle(command, CancellationToken.None));
        }
    }
}
