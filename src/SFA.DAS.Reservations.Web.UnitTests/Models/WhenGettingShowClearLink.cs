using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Reservations.Web.Models;

namespace SFA.DAS.Reservations.Web.UnitTests.Models
{
    public class WhenGettingShowClearLink
    {
        [Test]
        public void And_Has_SearchTerm_Then_True()
        {
            var filterModel = new ManageReservationsFilterModel
            {
                SearchTerm = "asedfas"
            };

            filterModel.ShowClearLink.Should().BeTrue();
        }

        [Test]
        public void And_Has_SelectedEmployer_Then_True()
        {
            var filterModel = new ManageReservationsFilterModel
            {
                SelectedEmployer = "asedfas"
            };

            filterModel.ShowClearLink.Should().BeTrue();
        }

        [Test]
        public void And_Has_SelectedCourse_Then_True()
        {
            var filterModel = new ManageReservationsFilterModel
            {
                SelectedCourse = "asedfas"
            };

            filterModel.ShowClearLink.Should().BeTrue();
        }

        [Test]
        public void And_Has_SelectedStartDate_Then_True()
        {
            var filterModel = new ManageReservationsFilterModel
            {
                SelectedStartDate = "asedfas"
            };

            filterModel.ShowClearLink.Should().BeTrue();
        }

        [Test]
        public void And_No_Search_Or_Filter_Then_False()
        {
            var filterModel = new ManageReservationsFilterModel();

            filterModel.ShowClearLink.Should().BeFalse();
        }
    }
}