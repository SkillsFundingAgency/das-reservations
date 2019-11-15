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

            request.SearchUrl.Should().Be($"{url}api/reservations/search?providerId={filter.ProviderId}&searchTerm={filter.Filter.SearchTerm}&pageNumber={filter.Filter.PageNumber}&pageItemCount={filter.Filter.PageSize}&selectedCourse={filter.Filter.SelectedCourse}");
        }
    }
}