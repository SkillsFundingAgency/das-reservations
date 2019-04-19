using System;
using NUnit.Framework;
using SFA.DAS.Reservations.Web.Infrastructure;
using SFA.DAS.Reservations.Web.Models;

namespace SFA.DAS.Reservations.Web.UnitTests.Models
{
    public class WhenConstructingAReviewViewModel
    {
        private const string AccountLegalEntityName = "Test Name";
        private const string AccountLegalEntityPublicHashedId = "123RDF";
        private const string CourseDescription = "Course 1";
        private const string StartDateDescription = "2019-01";

        [TestCase(null)]
        [TestCase((uint)1564564)]
        public void Then_The_Model_Is_Constructed_With_Correct_Route_Names(uint? ukPrn)
        {
            //Arrange
            var reservationsRouteModel = new ReservationsRouteModel
            {
                AccountLegalEntityPublicHashedId = AccountLegalEntityPublicHashedId,
                UkPrn = ukPrn,
                Id = new Guid(),
                EmployerAccountId = "123FDS",
                FromReview = true
            };
            
            //Act
            var actual = new ReviewViewModel(reservationsRouteModel,StartDateDescription,CourseDescription,AccountLegalEntityName,AccountLegalEntityPublicHashedId);

            //Assert
            Assert.AreEqual(AccountLegalEntityName,actual.AccountLegalEntityName);
            Assert.AreEqual(AccountLegalEntityPublicHashedId, actual.AccountLegalEntityPublicHashedId);
            Assert.AreEqual(CourseDescription, actual.CourseDescription);
            if (ukPrn == null)
            {
                Assert.AreEqual(ViewNames.EmployerReview, actual.ViewName);
                Assert.AreEqual(RouteNames.EmployerPostReview, actual.ConfirmRouteName);
                Assert.AreEqual(RouteNames.EmployerSelectCourse, actual.ChangeCourseRouteName);
                Assert.AreEqual(RouteNames.EmployerApprenticeshipTraining, actual.ChangeStartDateRouteName);
                Assert.AreEqual(string.Empty, actual.BackLink);
            }
            else
            {
                Assert.AreEqual(ViewNames.ProviderReview, actual.ViewName);
                Assert.AreEqual(RouteNames.ProviderPostReview, actual.ConfirmRouteName);
                Assert.AreEqual(RouteNames.ProviderApprenticeshipTraining, actual.ChangeCourseRouteName);
                Assert.AreEqual(RouteNames.ProviderApprenticeshipTraining, actual.ChangeStartDateRouteName);
                Assert.AreEqual(RouteNames.ProviderApprenticeshipTraining, actual.BackLink);
            }
            Assert.AreEqual(StartDateDescription, actual.StartDateDescription);
        }
    }
}
