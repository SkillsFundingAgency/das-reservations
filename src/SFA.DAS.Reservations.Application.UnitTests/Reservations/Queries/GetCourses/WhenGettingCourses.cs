using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.Reservations.Queries.GetCourses;
using SFA.DAS.Reservations.Domain.Courses;
using SFA.DAS.Reservations.Domain.Courses.Api;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Infrastructure.Api;
using SFA.DAS.Reservations.Infrastructure.Configuration.Configuration;

namespace SFA.DAS.Reservations.Application.UnitTests.Reservations.Queries.GetCourses
{
    public class WhenGettingCourses
    {
        private GetCoursesQueryHandler _handler;
        private Mock<IApiClient> _apiClient;
        private Mock<ICacheStorageService> _cacheService;
        private Mock<IOptions<ReservationsApiConfiguration>> _options;
        private const string ExpectedBaseUrl = "https://test.local/";
        private List<Course> _expectedApiCourses;
        private List<Course> _expectedCacheCourses;

        [SetUp]
        public void Arrange()
        {
            _expectedApiCourses = new List<Course>
            {
                new Course("1","Course 1",1),
                new Course("2","Course 2",2),
                new Course("3","Course 3",3)
            };
            
            _expectedCacheCourses = new List<Course>
            {
                new Course("5","Course 5",5),
                new Course("6","Course 6",6),
                new Course("7","Course 7",7)
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

            _handler = new GetCoursesQueryHandler(_apiClient.Object, _options.Object, _cacheService.Object);
        }

        [Test]
        public async Task Then_The_Courses_Are_Returned()
        {
            //Act
            var actual = await _handler.Handle(new GetCoursesQuery(), new CancellationToken());

            //Assert
            actual.Courses.Should().BeEquivalentTo(_expectedApiCourses);
        }

        [Test]
        public async Task Then_The_Courses_Are_Cached()
        {
            //Act
            await _handler.Handle(new GetCoursesQuery(), new CancellationToken());

            //Assert
            _cacheService.Verify(s => 
                    s.SaveToCache(nameof(GetCoursesQueryHandler), 
                        It.Is<GetCoursesResult>(r => 
                            r.Courses.First().Id.Equals(_expectedApiCourses.First().Id) &&
                            r.Courses.Skip(1).First().Id.Equals(_expectedApiCourses.Skip(1).First().Id) &&
                            r.Courses.Skip(2).First().Id.Equals(_expectedApiCourses.Skip(2).First().Id)), 24), Times.Once);
        }

        [Test]
        public async Task Then_The_Courses_Are_Retreieved_From_Cache()
        {
            //Arrange
            _cacheService.Setup(s => s.RetrieveFromCache<GetCoursesResult>(It.IsAny<string>()))
                         .ReturnsAsync(new GetCoursesResult{Courses = _expectedCacheCourses});

            //Act
            var actual = await _handler.Handle(new GetCoursesQuery(), new CancellationToken());

            //Assert
            _cacheService.Verify(s => s.RetrieveFromCache<GetCoursesResult>(nameof(GetCoursesQueryHandler)), 
                Times.Once);

            _apiClient.Verify(c => c.Get<CoursesApiRequest, GetCoursesResponse>(It.IsAny<CoursesApiRequest>()), 
                Times.Never);
            
            actual.Courses.Should().BeEquivalentTo(_expectedCacheCourses);
        }

        [Test]
        public async Task Then_The_Courses_Are_Retreieved_From_Api_If_Cache_Is_Empty()
        {
            //Act
            var actual = await _handler.Handle(new GetCoursesQuery(), new CancellationToken());

            //Assert
            _cacheService.Verify(s => s.RetrieveFromCache<GetCoursesResult>(nameof(GetCoursesQueryHandler)), 
                Times.Once);
            
            _apiClient.Verify(c => c.Get<CoursesApiRequest, GetCoursesResponse>(It.IsAny<CoursesApiRequest>()), 
                Times.Once);
           
            actual.Courses.Should().BeEquivalentTo(_expectedApiCourses);
        }
    }
}