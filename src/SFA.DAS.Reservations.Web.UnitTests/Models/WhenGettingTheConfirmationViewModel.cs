using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using SFA.DAS.Reservations.Domain.Courses;
using SFA.DAS.Reservations.Web.Models;

namespace SFA.DAS.Reservations.Web.UnitTests.Models
{
    public class WhenGettingTheConfirmationViewModel
    {
        [Test]
        public void Then_The_Model_Is_Constructed()
        {
            //Arrange
            var expectedReservationId = Guid.NewGuid();
            var expectedStartDate = new DateTime(2018, 10, 20);
            var expectedExpiryDate = new DateTime(2018, 12, 20);
            var expectedCourse = new Course("1", "Title", 0);

            //Act
            var actual = new ConfirmationViewModel(
                expectedReservationId,
                expectedStartDate,
                expectedExpiryDate,
                expectedCourse);

            //Assert
            Assert.AreEqual(expectedReservationId, actual.ReservationId);
            Assert.AreEqual(expectedStartDate, actual.StartDate);
            Assert.AreEqual(expectedExpiryDate, actual.ExpiryDate);
            Assert.AreEqual(expectedCourse, actual.Course);
        }

        [Test]
        public void Then_If_The_AddApprenticeUrl_Is_Available_The_Url_Is_Created()
        {
           
        }
    }
}
