using System;
using AutoFixture.NUnit3;
using NUnit.Framework;
using SFA.DAS.Reservations.Domain.Courses;
using SFA.DAS.Reservations.Web.Infrastructure;
using SFA.DAS.Reservations.Web.Models;

namespace SFA.DAS.Reservations.Web.UnitTests.Models
{
    public class WhenConstructingACompletedViewModel
    {

        private readonly Guid _expectedReservationId = Guid.NewGuid();
        private readonly DateTime _expectedStartDate = new DateTime(2018, 9, 20);
        private readonly DateTime _expectedExpiryDate = new DateTime(2018, 12, 20);
        private readonly Course _expectedCourse = new Course("1", "Title", 0);
        private const uint ExpectedProviderId = 4354351;
        private const string ExpectedHashedLegalEntityAccountId = "TGF45";

        [Test]
        public void Then_The_Model_Is_Constructed()
        {
            //Act
            var actual = new CompletedViewModel(
                _expectedReservationId,
                _expectedStartDate,
                _expectedExpiryDate,
                _expectedCourse,
                ExpectedHashedLegalEntityAccountId,
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
        public void Then_The_Model_Is_Constructed_With_White_Space()
        {
            //Act
            var actual = new CompletedViewModel(
                _expectedReservationId,
                _expectedStartDate,
                _expectedExpiryDate,
                _expectedCourse,
                ExpectedHashedLegalEntityAccountId,
                ExpectedProviderId," "," ");

            //Assert
            Assert.AreEqual(_expectedReservationId, actual.ReservationId);
            Assert.AreEqual(_expectedStartDate, actual.StartDate);
            Assert.AreEqual(_expectedExpiryDate, actual.ExpiryDate);
            Assert.AreEqual(_expectedCourse, actual.Course);
            Assert.IsFalse(actual.ShowDashboardUrl);
            Assert.IsFalse(actual.ShowApprenticeUrl);
            Assert.AreEqual(" ", actual.DashboardUrl);
            Assert.AreEqual("", actual.ApprenticeUrl);
        }

        [Test]
        public void Then_If_The_DashboardUrl_Is_Available_The_Url_Is_Created_If_Supplied()
        {
            //Arrange
            var expectedDashboardUrl = "https://test.local";

            //Act
            var actual = new CompletedViewModel(
                _expectedReservationId,
                _expectedStartDate,
                _expectedExpiryDate,
                _expectedCourse,
                ExpectedHashedLegalEntityAccountId,
                ExpectedProviderId,
                "",
                expectedDashboardUrl);

            //Act
            Assert.AreEqual(expectedDashboardUrl, actual.DashboardUrl);
            Assert.IsTrue(actual.ShowDashboardUrl);
        }

        [Test]
        public void Then_If_The_ApprenticeUrl_Is_Available_The_Url_Is_Created_If_Supplied()
        {

            //Act
            var actual = new CompletedViewModel(
                _expectedReservationId,
                _expectedStartDate,
                _expectedExpiryDate,
                _expectedCourse,
                ExpectedHashedLegalEntityAccountId,
                ExpectedProviderId,
                "",
                "","https://apprentice");

            //Act
            Assert.AreEqual($"https://apprentice/{ExpectedProviderId}/unapproved/add-apprentice?reservationId={_expectedReservationId}&employerAccountLegalEntityPublicHashedId={ExpectedHashedLegalEntityAccountId}&startMonthYear={_expectedStartDate.ToString("MMyyyy")}&courseCode={_expectedCourse.Id}", actual.ApprenticeUrl);
            Assert.IsTrue(actual.ShowApprenticeUrl);
        }

        [Test]
        public void Then_The_ApprenticeUrl_Is_Built_With_No_Course_Supplied()
        {
            var actual = new CompletedViewModel(
                _expectedReservationId,
                _expectedStartDate,
                _expectedExpiryDate,
                null,
                ExpectedHashedLegalEntityAccountId,
                ExpectedProviderId,
                "",
                "", "https://apprentice");

            //Act
            Assert.AreEqual($"https://apprentice/{ExpectedProviderId}/unapproved/add-apprentice?reservationId={_expectedReservationId}&employerAccountLegalEntityPublicHashedId={ExpectedHashedLegalEntityAccountId}&startMonthYear={_expectedStartDate.ToString("MMyyyy")}", actual.ApprenticeUrl);
            Assert.IsTrue(actual.ShowApprenticeUrl);
        }

        [Test]
        public void Then_If_The_EmployerDashboardUrl_Is_Available_The_Url_Is_Created_If_Supplied()
        {
            //Arrange
            var expectedEmployerDashboardUrl = "https://test.local";

            //Act
            var actual = new CompletedViewModel(
                _expectedReservationId,
                _expectedStartDate,
                _expectedExpiryDate,
                _expectedCourse,
                ExpectedHashedLegalEntityAccountId,
                ExpectedProviderId, 
                employerDashboardUrl:expectedEmployerDashboardUrl);

            //Act
            Assert.AreEqual(expectedEmployerDashboardUrl, actual.EmployerDashboardUrl);
        }

        [Test, AutoData]
        public void And_Ukprn_Is_Null_Then_ViewName_Is_EmployerCompleted(
            ReservationsRouteModel routeModel,
            Course course)
        {
            routeModel.UkPrn = null;
            var viewModel = new CompletedViewModel(Guid.NewGuid(), DateTime.Today, DateTime.Today, course, "234");
            Assert.AreEqual(ViewNames.EmployerCompleted, viewModel.ViewName);
        }

        [Test, AutoData]
        public void And_Ukprn_Not_Null_Then_ViewName_Is_ProviderCompleted(
            CompletedViewModel viewModel)
        {
            Assert.AreEqual(ViewNames.ProviderCompleted, viewModel.ViewName);
        }
    }
}
