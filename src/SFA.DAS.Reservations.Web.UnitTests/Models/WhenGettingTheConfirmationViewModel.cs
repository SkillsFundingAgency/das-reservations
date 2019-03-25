using System;
using NUnit.Framework;
using SFA.DAS.Reservations.Domain.Courses;
using SFA.DAS.Reservations.Web.Models;

namespace SFA.DAS.Reservations.Web.UnitTests.Models
{
    public class WhenGettingTheConfirmationViewModel
    {

        private readonly Guid _expectedReservationId = Guid.NewGuid();
        private readonly DateTime _expectedStartDate = new DateTime(2018, 10, 20);
        private readonly DateTime _expectedExpiryDate = new DateTime(2018, 12, 20);
        private readonly Course _expectedCourse = new Course("1", "Title", 0);
        private const uint ExpectedProviderId = 4354351;

        [Test]
        public void Then_The_Model_Is_Constructed()
        {
            //Act
            var actual = new ConfirmationViewModel(
                _expectedReservationId,
                _expectedStartDate,
                _expectedExpiryDate,
                _expectedCourse,
                ExpectedProviderId);

            //Assert
            Assert.AreEqual(_expectedReservationId, actual.ReservationId);
            Assert.AreEqual(_expectedStartDate, actual.StartDate);
            Assert.AreEqual(_expectedExpiryDate, actual.ExpiryDate);
            Assert.AreEqual(_expectedCourse, actual.Course);
            Assert.IsFalse(actual.ShowDashboardUrl);
            Assert.IsFalse(actual.ShowApprenticeUrl);
            Assert.AreEqual(string.Empty,actual.DashboardUrl);
            Assert.AreEqual(string.Empty,actual.ApprenticeUrl);
        }

        [Test]
        public void Then_If_The_DashboardUrl_Is_Available_The_Url_Is_Created_If_Supplied()
        {
            //Arrange
            var expectedDashboardUrl = "https://test.local";

            //Act
            var actual = new ConfirmationViewModel(
                _expectedReservationId,
                _expectedStartDate,
                _expectedExpiryDate,
                _expectedCourse,
                ExpectedProviderId,
                expectedDashboardUrl);

            //Act
            Assert.AreEqual(expectedDashboardUrl, actual.DashboardUrl);
            Assert.IsTrue(actual.ShowDashboardUrl);
        }

        [Test]
        public void Then_If_The_ApprenticeUrl_Is_Available_The_Url_Is_Created_If_Supplied()
        {

            //Act
            var actual = new ConfirmationViewModel(
                _expectedReservationId,
                _expectedStartDate,
                _expectedExpiryDate,
                _expectedCourse,
                ExpectedProviderId,
                "","https://apprentice");

            //Act
            Assert.AreEqual($"https://apprentice/{ExpectedProviderId}/unapproved/add-apprentice?reservationId={_expectedReservationId}&employerAccountLegalEntityPublicHashedId=YZWX27&courseCode={_expectedCourse.Id}&startMonthYear={_expectedStartDate.Month}{_expectedStartDate.Year}", actual.ApprenticeUrl);
            Assert.IsTrue(actual.ShowApprenticeUrl);
        }

        [Test]
        public void Then_The_ApprenticeUrl_Is_Built_With_No_Course_Supplied()
        {
            var actual = new ConfirmationViewModel(
                _expectedReservationId,
                _expectedStartDate,
                _expectedExpiryDate,
                null,
                ExpectedProviderId,
                "", "https://apprentice");

            //Act
            Assert.AreEqual($"https://apprentice/{ExpectedProviderId}/unapproved/add-apprentice?reservationId={_expectedReservationId}&employerAccountLegalEntityPublicHashedId=YZWX27&startMonthYear={_expectedStartDate.Month}{_expectedStartDate.Year}", actual.ApprenticeUrl);
            Assert.IsTrue(actual.ShowApprenticeUrl);
        }
    }
}
