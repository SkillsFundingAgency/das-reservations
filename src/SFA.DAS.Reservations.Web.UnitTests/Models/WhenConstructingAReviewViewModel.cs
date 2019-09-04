using System;
using NUnit.Framework;
using SFA.DAS.Reservations.Domain.Rules;
using SFA.DAS.Reservations.Web.Infrastructure;
using SFA.DAS.Reservations.Web.Models;

namespace SFA.DAS.Reservations.Web.UnitTests.Models
{
    public class WhenConstructingAReviewViewModel
    {
        private const string AccountLegalEntityName = "Test Name";
        private const string AccountLegalEntityPublicHashedId = "123RDF";
        private const string CourseDescription = "Course 1";
        private const bool ExpectedReserve = true;
       

        [TestCase(null)]
        [TestCase((uint)1564564)]
        public void Then_The_Model_Is_Constructed_With_Correct_Route_Names(uint? ukPrn)
        {
            //Arrange
            var startDate = new TrainingDateModel{StartDate = DateTime.Now};

            var reservationsRouteModel = new ReservationsRouteModel
            {
                AccountLegalEntityPublicHashedId = AccountLegalEntityPublicHashedId,
                UkPrn = ukPrn,
                Id = new Guid(),
                EmployerAccountId = "123FDS",
                FromReview = true
            };
            
            //Act
            var actual = new ReviewViewModel(reservationsRouteModel, startDate,CourseDescription,AccountLegalEntityName,AccountLegalEntityPublicHashedId);

            //Assert
            AssertAllProperties(actual, ukPrn, startDate);
        }

        [TestCase(null)]
        [TestCase((uint)1564564)]
        public void Then_The_Model_Is_Constructed_With_Correct_Route_Names_Using_Other_Constructor(uint? ukPrn)
        {
            //Arrange
            var startDate = new TrainingDateModel{StartDate = DateTime.Now};

            var reservationsRouteModel = new ReservationsRouteModel
            {
                AccountLegalEntityPublicHashedId = AccountLegalEntityPublicHashedId,
                UkPrn = ukPrn,
                Id = new Guid(),
                EmployerAccountId = "123FDS",
                FromReview = true
            };

            var postReviewViewModel = new PostReviewViewModel
            {
                AccountLegalEntityName = AccountLegalEntityName,
                AccountLegalEntityPublicHashedId = AccountLegalEntityPublicHashedId,
                CourseDescription = CourseDescription,
                StartDate = startDate.StartDate,
                Reserve = ExpectedReserve
            };
            
            //Act
            var actual = new ReviewViewModel(reservationsRouteModel, postReviewViewModel);

            //Assert
            AssertAllProperties(actual, ukPrn, startDate);
            Assert.AreEqual(ExpectedReserve, actual.Reserve);
        }

        private void AssertAllProperties(ReviewViewModel actual, uint? ukPrn, TrainingDateModel startDate)
        {
            Assert.AreEqual(AccountLegalEntityName,actual.AccountLegalEntityName);
            Assert.AreEqual(AccountLegalEntityPublicHashedId, actual.AccountLegalEntityPublicHashedId);
            Assert.AreEqual(CourseDescription, actual.CourseDescription);
            if (ukPrn == null)
            {
                Assert.AreEqual(ViewNames.EmployerReview, actual.ViewName);
                Assert.AreEqual(RouteNames.EmployerPostReview, actual.ConfirmRouteName);
                Assert.AreEqual(RouteNames.EmployerSelectCourse, actual.ChangeCourseRouteName);
                Assert.AreEqual(RouteNames.EmployerApprenticeshipTraining, actual.ChangeStartDateRouteName);
                Assert.AreEqual(RouteNames.EmployerApprenticeshipTraining, actual.BackLink);
            }
            else
            {
                Assert.AreEqual(ViewNames.ProviderReview, actual.ViewName);
                Assert.AreEqual(RouteNames.ProviderPostReview, actual.ConfirmRouteName);
                Assert.AreEqual(RouteNames.ProviderApprenticeshipTraining, actual.ChangeCourseRouteName);
                Assert.AreEqual(RouteNames.ProviderApprenticeshipTraining, actual.ChangeStartDateRouteName);
                Assert.AreEqual(RouteNames.ProviderApprenticeshipTraining, actual.BackLink);
            }
            Assert.AreEqual(startDate, actual.TrainingDate);
        }
    }
}
