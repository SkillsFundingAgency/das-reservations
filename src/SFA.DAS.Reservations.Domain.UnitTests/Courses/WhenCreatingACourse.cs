using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Reservations.Domain.Courses;

namespace SFA.DAS.Reservations.Domain.UnitTests.Courses
{
    public class WhenCreatingACourse
    {

        [Test]
        public void WhenConstructorParametersArePassed_ThenTheCourseSetsTheValues()
        {
            //Arrange
            var expectedCourseId = "1-21-1";
            var expectedCourseTitle = "Course 1";
            var expectedCourseLevel = 4;


            //Act
            var actual = new Course (expectedCourseId, expectedCourseTitle,expectedCourseLevel );

            //Assert
            expectedCourseId.Should().Be(actual.Id);
            expectedCourseTitle.Should().Be(actual.Title);
            expectedCourseLevel.Should().Be(actual.Level);
        }

        [TestCase("")]
        [TestCase(null)]
        public void WhenCourseTitleIsEmptyOrNull_SetCourseTitleToUnknown(string expectedTitle)
        {
            //Arrange
            var expectedString = "Unknown";

            //Act
            var course = new Course("",expectedTitle,3);

            //Assert
            expectedString.Should().Be(course.CourseDescription);
            expectedString.Should().Be(course.Title);

        }

        [Test]
        public void Then_The_Course_Description_Is_Taken_From_The_Title_And_Level()
        {
            //Arrange Act
            var actualApprenticeship = new Course("", "Some title", 1);

            //Assert
            actualApprenticeship.CourseDescription.Should().Be("Some title - Level 1");
        }

        [Test]
        public void Then_The_Empty_Constructor_Sets_The_Title_To_Unknown()
        {
            //Arrange
            var expectedString = "Unknown";

            //Act
            var course = new Course(null, null, 0);

            //Assert
            course.CourseDescription.Should().Be(expectedString);
        }
    }
}