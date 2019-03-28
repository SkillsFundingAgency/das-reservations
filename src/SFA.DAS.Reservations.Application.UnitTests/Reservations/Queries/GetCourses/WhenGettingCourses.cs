using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.Reservations.Queries.GetCourses;
using SFA.DAS.Reservations.Domain.Courses;
using SFA.DAS.Reservations.Domain.Courses.Api;
using SFA.DAS.Reservations.Infrastructure.Api;
using SFA.DAS.Reservations.Infrastructure.Configuration.Configuration;

namespace SFA.DAS.Reservations.Application.UnitTests.Reservations.Queries.GetCourses
{
    public class WhenGettingCourses
    {
        private GetCoursesQueryHandler _handler;
        private Mock<IApiClient> _apiClient;
        private Mock<IOptions<ReservationsApiConfiguration>> _options;
        private const string ExpectedBaseUrl = "https://test.local/";
        private List<Course> _expectedCourses;

        [SetUp]
        public void Arrange()
        {
            _expectedCourses = new List<Course>
            {
                new Course("1","Course 1",1),
                new Course("2","Course 2",2),
                new Course("3","Course 3",3),
                
            };

            _apiClient = new Mock<IApiClient>();
            _apiClient.Setup(x =>
                    x.Get<GetCoursesResponse>(
                        It.Is<CoursesApiRequest>(c =>
                            c.GetUrl.Equals(
                                $"{ExpectedBaseUrl}api/courses"))))
                .ReturnsAsync(new GetCoursesResponse
                {
                   Courses = _expectedCourses
                });

            _options = new Mock<IOptions<ReservationsApiConfiguration>>();
            _options.Setup(x => x.Value.Url).Returns(ExpectedBaseUrl);

            _handler = new GetCoursesQueryHandler(_apiClient.Object, _options.Object);
        }

        [Test]
        public async Task Then_The_Courses_Are_Returned()
        {
            //Arrange
            var command = new GetCoursesQuery();

            //Act
            var actual = await _handler.Handle(command, new CancellationToken());

            //Assert
            actual.Courses.Should().BeEquivalentTo(_expectedCourses);
        }
    }
}