using NUnit.Framework;
using SFA.DAS.Reservations.Domain.Courses;
using System;
using System.Collections.Generic;
using System.Text;

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
            Assert.AreEqual(expectedCourseId, actual.Id);
            Assert.AreEqual(expectedCourseTitle, actual.Title);
            Assert.AreEqual(expectedCourseLevel, actual.Level);
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
            Assert.AreEqual(expectedString, course.CourseDescription);
            Assert.AreEqual(expectedString, course.Title);

        }

        [Test]
        public void Then_The_Course_Description_Is_Taken_From_The_Title_And_Level()
        {
            //Arrange Act
            var actualApprenticeship = new Course("", "Some title", 1);

            //Assert
            Assert.AreEqual("Some title - Level 1", actualApprenticeship.CourseDescription);
        }

    }
}
