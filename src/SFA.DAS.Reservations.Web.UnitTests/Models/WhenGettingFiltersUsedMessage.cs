using AutoFixture.NUnit3;
using FluentAssertions;
using Microsoft.AspNetCore.Html;
using NUnit.Framework;
using SFA.DAS.Reservations.Web.Models;

namespace SFA.DAS.Reservations.Web.UnitTests.Models
{
    public class WhenGettingFiltersUsedMessage
    {
        [Test]
        public void And_No_Search_And_No_Filters_Then_Null()
        {
            var filterModel = new ManageReservationsFilterModel();

            filterModel.FiltersUsedMessage.Should().Be(HtmlString.Empty);
        }

        [Test, AutoData]
        public void And_Search_And_No_Filters_Then_Quoted_SearchTerm(
            string searchTerm)
        {
            var filterModel = new ManageReservationsFilterModel
            {
                SearchTerm = searchTerm
            };

            filterModel.FiltersUsedMessage.Value.Should().Be($"matching <strong>‘{searchTerm}’</strong>");
        }

        [Test, AutoData]
        public void And_No_Search_And_SelectedEmployer_Then_SelectedEmployer(
            string selectedEmployer)
        {
            var filterModel = new ManageReservationsFilterModel
            {
                SelectedEmployer = selectedEmployer
            };

            filterModel.FiltersUsedMessage.Value.Should().Be($"matching <strong>{selectedEmployer}</strong>");
        }

        [Test, AutoData]
        public void And_No_Search_And_SelectedCourse_Then_SelectedCourse(
            string selectedCourse)
        {
            var filterModel = new ManageReservationsFilterModel
            {
                SelectedCourse = selectedCourse
            };

            filterModel.FiltersUsedMessage.Value.Should().Be($"matching <strong>{selectedCourse}</strong>");
        }

        [Test, AutoData]
        public void And_No_Search_And_SelectedStartDate_Then_SelectedStartDate(
            string selectedStartDate)
        {
            var filterModel = new ManageReservationsFilterModel
            {
                SelectedStartDate = selectedStartDate
            };

            filterModel.FiltersUsedMessage.Value.Should().Be($"matching <strong>{selectedStartDate}</strong>");
        }

        [Test, AutoData]
        public void And_Search_And_SelectedEmployer_Then_SearchTerm_And_SelectedEmployer(
            string searchTerm,
            string selectedEmployer)
        {
            var filterModel = new ManageReservationsFilterModel
            {
                SearchTerm = searchTerm,
                SelectedEmployer = selectedEmployer
            };

            filterModel.FiltersUsedMessage.Value
                .Should().Be($"matching <strong>‘{searchTerm}’</strong>" +
                             $" and <strong>{selectedEmployer}</strong>");
        }

        [Test, AutoData]
        public void And_Search_And_SelectedEmployer_And_SelectedCourse_Then_SearchTerm_Comma_SelectedEmployer_And_SelectedCourse(
            string searchTerm,
            string selectedEmployer,
            string selectedCourse)
        {
            var filterModel = new ManageReservationsFilterModel
            {
                SearchTerm = searchTerm,
                SelectedEmployer = selectedEmployer,
                SelectedCourse = selectedCourse
            };

            filterModel.FiltersUsedMessage.Value
                .Should().Be($"matching <strong>‘{searchTerm}’</strong>" +
                             $", <strong>{selectedEmployer}</strong>" +
                             $" and <strong>{selectedCourse}</strong>");
        }

        [Test, AutoData]
        public void And_SelectedEmployer_And_SelectedCourse_And_SelectedStartDate_Then_SelectedEmployer_Comma_SelectedCourse_And_SelectedStartDate(
            string selectedEmployer,
            string selectedCourse,
            string selectedStartDate)
        {
            var filterModel = new ManageReservationsFilterModel
            {
                SelectedEmployer = selectedEmployer,
                SelectedCourse = selectedCourse,
                SelectedStartDate = selectedStartDate
            };

            filterModel.FiltersUsedMessage.Value
                .Should().Be($"matching <strong>{selectedEmployer}</strong>" +
                             $", <strong>{selectedCourse}</strong>" +
                             $" and <strong>{selectedStartDate}</strong>");
        }
    }
}