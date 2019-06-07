using System;
using NUnit.Framework;
using SFA.DAS.Reservations.Domain.Rules;

namespace SFA.DAS.Reservations.Web.UnitTests.Models
{
    public class WhenComparingTrainingDateModels
    {
        [Test]
        public void ThenWillReturnSameIfBothDatesAreSetAndEqual()
        {
            var source = new TraningDateModel {StartDate = DateTime.Now, EndDate = DateTime.Now.AddDays(1)};
            var target = new TraningDateModel {StartDate = DateTime.Now, EndDate = DateTime.Now.AddDays(1)};

            Assert.IsTrue(source.Equals(target));
        }

        [Test]
        public void ThenWillReturnSameIfStartDateSetAndEqual()
        {
            var source = new TraningDateModel {StartDate = DateTime.Now};
            var target = new TraningDateModel {StartDate = DateTime.Now};

            Assert.IsTrue(source.Equals(target));
        }

        [Test]
        public void ThenWillReturnDifferentIfStartDatesAreSetAndNotEqual()
        {
            var source = new TraningDateModel {StartDate = DateTime.Now, EndDate = DateTime.Now.AddDays(1)};
            var target = new TraningDateModel {StartDate = DateTime.Now.AddDays(-1), EndDate = DateTime.Now.AddDays(1)};

            Assert.IsFalse(source.Equals(target));
        }

        [Test]
        public void ThenWillReturnDifferentIfEndDatesAreSetAndNotEqual()
        {
            var source = new TraningDateModel {StartDate = DateTime.Now, EndDate = DateTime.Now.AddDays(1)};
            var target = new TraningDateModel {StartDate = DateTime.Now, EndDate = DateTime.Now.AddDays(2)};

            Assert.IsFalse(source.Equals(target));
        }

        [Test]
        public void ThenWillReturnDifferentIfTargetIsNotSameType()
        {
            var source = new TraningDateModel {StartDate = DateTime.Now, EndDate = DateTime.Now.AddDays(1)};

            Assert.IsFalse(source.Equals(123));
        }

        [Test]
        public void ThenWillReturnSameIfBothAreUnset()
        {
            var source = new TraningDateModel();
            var target = new TraningDateModel();

            Assert.IsTrue(source.Equals(target));
        }
    }
}
