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
            string expectedCourseId = "1-21-1";
            string expectedCourseTitle = "Course 1";
            int expectedCourseLevel = 4;


            //Act
            Course actual = new Course (expectedCourseId, expectedCourseTitle,expectedCourseLevel );

            //Assert
            Assert.AreEqual(expectedCourseId, actual.Id);
            Assert.AreEqual(expectedCourseTitle, actual.Title);
            Assert.AreEqual(expectedCourseLevel, actual.Level);
        }

        [Test]
        public void WhenCourseTitleIsEmpty_SetCourseTitleToUnknown()
        {
            //Arrange
            string expectedString = "Unknown";

            //Act
            Course course = new Course("1-22-1","",3);

            //Assert
            Assert.AreEqual(expectedString, course.Title);


        }

        [Test]
        public void WhenCourseTitleIsNull_SetCourseTitleToUnknown()
        {
            //Arrange
            string expectedString = "Unknown";

            //Act
            Course course = new Course("1-22-1",null,3);

            //Assert
            Assert.AreEqual(expectedString, course.Title);


        }
    }
}
