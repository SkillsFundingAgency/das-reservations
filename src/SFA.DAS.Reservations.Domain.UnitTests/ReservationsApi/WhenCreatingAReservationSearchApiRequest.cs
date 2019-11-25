using AutoFixture.NUnit3;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Reservations.Domain.Reservations;
using SFA.DAS.Reservations.Domain.Reservations.Api;

namespace SFA.DAS.Reservations.Domain.UnitTests.ReservationsApi
{
    [TestFixture]
    public class WhenCreatingAReservationSearchApiRequest
    {
        [Test, AutoData]
        public void Then_It_Sets_The_SearchUrl(
            string url,
            SearchReservationsRequest filter)
        {
            var request = new SearchReservationsApiRequest(url, filter);

            request.SearchUrl.Should().Be($"{url}api/reservations/search" +
                                          $"?providerId={filter.ProviderId}" +
                                          $"&pageNumber={filter.Filter.PageNumber}" +
                                          $"&pageItemCount={filter.Filter.PageSize}" +
                                          $"&searchTerm={filter.Filter.SearchTerm}" +
                                          $"&selectedEmployer={filter.Filter.SelectedEmployer}" +
                                          $"&selectedCourse={filter.Filter.SelectedCourse}" +
                                          $"&selectedStartDate={filter.Filter.SelectedStartDate}");
        }

        [Test, AutoData]
        public void And_No_SearchTerm_Then_Not_In_SearchUrl(
            string url,
            SearchReservationsRequest filter)
        {
            filter.Filter.SearchTerm = null;

            var request = new SearchReservationsApiRequest(url, filter);

            request.SearchUrl.Should().Be($"{url}api/reservations/search" +
                                          $"?providerId={filter.ProviderId}" +
                                          $"&pageNumber={filter.Filter.PageNumber}" +
                                          $"&pageItemCount={filter.Filter.PageSize}" +
                                          $"&selectedEmployer={filter.Filter.SelectedEmployer}" +
                                          $"&selectedCourse={filter.Filter.SelectedCourse}" +
                                          $"&selectedStartDate={filter.Filter.SelectedStartDate}");
        }

        [Test, AutoData]
        public void And_No_SelectedEmployer_Then_Not_In_SearchUrl(
            string url,
            SearchReservationsRequest filter)
        {
            filter.Filter.SelectedEmployer = null;

            var request = new SearchReservationsApiRequest(url, filter);

            request.SearchUrl.Should().Be($"{url}api/reservations/search" +
                                          $"?providerId={filter.ProviderId}" +
                                          $"&pageNumber={filter.Filter.PageNumber}" +
                                          $"&pageItemCount={filter.Filter.PageSize}" +
                                          $"&searchTerm={filter.Filter.SearchTerm}" +
                                          $"&selectedCourse={filter.Filter.SelectedCourse}" +
                                          $"&selectedStartDate={filter.Filter.SelectedStartDate}");
        }

        [Test, AutoData]
        public void And_No_SelectedCourse_Then_Not_In_SearchUrl(
            string url,
            SearchReservationsRequest filter)
        {
            filter.Filter.SelectedCourse = null;

            var request = new SearchReservationsApiRequest(url, filter);

            request.SearchUrl.Should().Be($"{url}api/reservations/search" +
                                          $"?providerId={filter.ProviderId}" +
                                          $"&pageNumber={filter.Filter.PageNumber}" +
                                          $"&pageItemCount={filter.Filter.PageSize}" +
                                          $"&searchTerm={filter.Filter.SearchTerm}" +
                                          $"&selectedEmployer={filter.Filter.SelectedEmployer}" +
                                          $"&selectedStartDate={filter.Filter.SelectedStartDate}");
        }

        [Test, AutoData]
        public void And_No_SelectedStartDate_Then_Not_In_SearchUrl(
            string url,
            SearchReservationsRequest filter)
        {
            filter.Filter.SelectedStartDate = null;

            var request = new SearchReservationsApiRequest(url, filter);

            request.SearchUrl.Should().Be($"{url}api/reservations/search" +
                                          $"?providerId={filter.ProviderId}" +
                                          $"&pageNumber={filter.Filter.PageNumber}" +
                                          $"&pageItemCount={filter.Filter.PageSize}" +
                                          $"&searchTerm={filter.Filter.SearchTerm}" +
                                          $"&selectedEmployer={filter.Filter.SelectedEmployer}" +
                                          $"&selectedCourse={filter.Filter.SelectedCourse}");
        }

        [Test, AutoData]
        public void And_No_SearchTerm_Or_Filters_Then_Not_In_SearchUrl(
            string url,
            SearchReservationsRequest filter)
        {
            filter.Filter.SearchTerm = null;
            filter.Filter.SelectedEmployer = null;
            filter.Filter.SelectedCourse = null;
            filter.Filter.SelectedStartDate = null;

            var request = new SearchReservationsApiRequest(url, filter);

            request.SearchUrl.Should().Be($"{url}api/reservations/search" +
                                          $"?providerId={filter.ProviderId}" +
                                          $"&pageNumber={filter.Filter.PageNumber}" +
                                          $"&pageItemCount={filter.Filter.PageSize}");
        }
    }
}