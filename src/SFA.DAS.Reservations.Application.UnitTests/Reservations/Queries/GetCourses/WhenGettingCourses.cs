using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.Reservations.Queries.GetCourses;
using SFA.DAS.Reservations.Application.Reservations.Services;
using SFA.DAS.Reservations.Domain.Courses;

namespace SFA.DAS.Reservations.Application.UnitTests.Reservations.Queries.GetCourses
{
    public class WhenGettingCourses
    {
        private GetCoursesQueryHandler _handler;
        private Mock<ICourseService> _service;
        private List<Course> _expectedCourses;

        [SetUp]
        public void Arrange()
        {
            _expectedCourses = new List<Course>
            {
                new Course("1","Course 1",1),
                new Course("2","Course 2",2),
                new Course("3","Course 3",3)
            };
            
            _service = new Mock<ICourseService>();
            _service.Setup(s => s.GetCourses()).ReturnsAsync(_expectedCourses);

            _handler = new GetCoursesQueryHandler(_service.Object);
        }

        [Test]
        public async Task Then_The_Courses_Are_Returned()
        {
            //Act
            var actual = await _handler.Handle(new GetCoursesQuery(), new CancellationToken());

            //Assert
            actual.Courses.Should().BeEquivalentTo(_expectedCourses);
        }
    }
}