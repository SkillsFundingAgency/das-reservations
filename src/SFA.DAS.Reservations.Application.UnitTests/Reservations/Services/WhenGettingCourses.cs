using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.Exceptions;
using SFA.DAS.Reservations.Application.Reservations.Services;
using SFA.DAS.Reservations.Domain.Courses;
using SFA.DAS.Reservations.Domain.Courses.Api;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Infrastructure.Api;
using SFA.DAS.Reservations.Infrastructure.Configuration.Configuration;
using SFA.DAS.Reservations.Infrastructure.Exceptions;

namespace SFA.DAS.Reservations.Application.UnitTests.Reservations.Services
{
    public class WhenGettingCourses
    {
        private ICourseService _service;
        private Mock<IApiClient> _apiClient;
        private Mock<ICacheStorageService> _cacheService;
        private Mock<IOptions<ReservationsApiConfiguration>> _options;
        private const string ExpectedBaseUrl = "https://test.local/";
        private List<Course> _expectedApiCourses;
        private Dictionary<string, Course> _expectedCacheCourses;

        [SetUp]
        public void Arrange()
        {
            _expectedApiCourses = new List<Course>
            {
                new Course("1","Course 1",1),
                new Course("2","Course 2",2),
                new Course("3","Course 3",3)
            };
            
            _expectedCacheCourses = new Dictionary<string, Course>
            {
                {"5", new Course("5","Course 5",5)},
                {"6", new Course("6","Course 6",6)},
                {"7", new Course("7","Course 7",7)}
            };

            _apiClient = new Mock<IApiClient>();
            _apiClient.Setup(x =>
                    x.Get<CoursesApiRequest, GetCoursesResponse>(
                        It.Is<CoursesApiRequest>(c =>
                            c.GetUrl.Equals(
                                $"{ExpectedBaseUrl}api/courses"))))
                .ReturnsAsync(new GetCoursesResponse
                {
                   Courses = _expectedApiCourses
                });

            _cacheService = new Mock<ICacheStorageService>();

            _options = new Mock<IOptions<ReservationsApiConfiguration>>();
            _options.Setup(x => x.Value.Url).Returns(ExpectedBaseUrl);

            _service = new CourseService(_apiClient.Object, _options.Object, _cacheService.Object);
        }

        [Test]
        public async Task Then_The_Courses_Are_Returned()
        {
            //Act
            var courses = await _service.GetCourses();

            //Assert
            courses.Should().BeEquivalentTo(_expectedApiCourses);
        }

        [Test]
        public async Task Then_The_Courses_Are_Cached()
        {
            //Act
            await _service.GetCourses();

            //Assert
            _cacheService.Verify(s => 
                    s.SaveToCache(nameof(CourseService), 
                        It.Is<IDictionary<string, Course>>(r => 
                            r.First().Key.Equals(_expectedApiCourses.First().Id) &&
                            r.Skip(1).First().Key.Equals(_expectedApiCourses.Skip(1).First().Id) &&
                            r.Skip(2).First().Key.Equals(_expectedApiCourses.Skip(2).First().Id)), 24), Times.Once);
        }

        [Test]
        public async Task Then_The_Courses_Are_Retreieved_From_Cache()
        {
            //Arrange
            _cacheService.Setup(s => s.RetrieveFromCache<IDictionary<string, Course>>(It.IsAny<string>()))
                         .ReturnsAsync(_expectedCacheCourses);

            //Act
            var courses = await _service.GetCourses();

            //Assert
            _cacheService.Verify(s => s.RetrieveFromCache<IDictionary<string, Course>>(nameof(CourseService)), 
                Times.Once);

            _apiClient.Verify(c => c.Get<CoursesApiRequest, GetCoursesResponse>(It.IsAny<CoursesApiRequest>()), 
                Times.Never);
            
            courses.Should().BeEquivalentTo(_expectedCacheCourses.Values);
        }

        [Test]
        public async Task Then_The_Courses_Are_Retreieved_From_Api_If_Cache_Is_Empty()
        {
            //Act
            var courses = await _service.GetCourses();

            //Assert
            _cacheService.Verify(s => s.RetrieveFromCache<IDictionary<string, Course>>(nameof(CourseService)), 
                Times.Once);
            
            _apiClient.Verify(c => c.Get<CoursesApiRequest, GetCoursesResponse>(It.IsAny<CoursesApiRequest>()), 
                Times.Once);
           
            courses.Should().BeEquivalentTo(_expectedApiCourses);
        }

        [Test]
        public async Task Then_The_Course_Is_Returned()
        {
            //Assign
            _cacheService.Setup(s => s.RetrieveFromCache<IDictionary<string, Course>>(It.IsAny<string>()))
                .ReturnsAsync(_expectedCacheCourses);

            //Act
            var course = await _service.GetCourse("6");

            //Assert
            course.Should().BeEquivalentTo(_expectedCacheCourses["6"]);
        }

        [Test]
        public void Then_Throws_Exception_If_Course_Does_Not_Exist()
        {
            //Assign
            _cacheService.Setup(s => s.RetrieveFromCache<IDictionary<string, Course>>(It.IsAny<string>()))
                .ReturnsAsync(_expectedCacheCourses);

            //Act + Assert
            Assert.ThrowsAsync<CourseNotFoundException>(() => _service.GetCourse("20"));
        }

        [Test]
        public async Task Then_Can_Find_Out_Course_Exists()
        {
            //Assign
            _cacheService.Setup(s => s.RetrieveFromCache<IDictionary<string, Course>>(It.IsAny<string>()))
                .ReturnsAsync(_expectedCacheCourses);

            //Act
            var exists = await _service.CourseExists("6");

            //Assert
            Assert.IsTrue(exists);
        }

        [Test]
        public async Task Then_Can_Find_Out_Course_Does_Not_Exists()
        {
            //Assign
            _cacheService.Setup(s => s.RetrieveFromCache<IDictionary<string, Course>>(It.IsAny<string>()))
                .ReturnsAsync(_expectedCacheCourses);

            //Act
            var exists = await _service.CourseExists("60");

            //Assert
            Assert.IsFalse(exists);
        }
    }
}
