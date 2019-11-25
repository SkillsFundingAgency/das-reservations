using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.Reservations.Services;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Domain.Reservations;
using SFA.DAS.Reservations.Domain.Reservations.Api;
using SFA.DAS.Reservations.Infrastructure.Api;
using SFA.DAS.Reservations.Infrastructure.Configuration;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Reservations.Application.UnitTests.Reservations.Services
{
    [TestFixture]
    public class WhenCallingSearchReservations
    {
        [Test, MoqAutoData]
        public async Task Then_Gets_Reservations_From_Api(
            SearchReservationsRequest searchRequest,
            SearchReservationsApiResponse reservationsApiResponse,
            [Frozen] Mock<IOptions<ReservationsApiConfiguration>> mockOptions,
            [Frozen] Mock<IApiClient> mockApiClient,
            ReservationService service)
        {
            var dates = new List<DateTime>
            {
                DateTime.Parse("Apr 2020"), 
                DateTime.Parse("Sep 2010"),
                DateTime.Parse("Aug 2018"),
                DateTime.Parse("Oct 2017"),
                DateTime.Parse("Jul 2019")
            };
            reservationsApiResponse.Filters.StartDateFilters = dates.Select(dt => $"{dt:MMM yyyy} to {dt.AddMonths(3):MMM yyyy}");
            mockApiClient
                .Setup(client => client.Search<SearchReservationsApiResponse>(It.IsAny<ISearchApiRequest>()))
                .ReturnsAsync(reservationsApiResponse);

            await service.SearchReservations(searchRequest);

            mockApiClient.Verify(client => client.Search<SearchReservationsApiResponse>(
                    It.Is<ISearchApiRequest>(request =>
                        request.SearchUrl.StartsWith(mockOptions.Object.Value.Url) &&
                        request.SearchUrl.Contains(searchRequest.ProviderId.ToString()) &&
                        request.SearchUrl.Contains(searchRequest.Filter.SearchTerm) &&
                        request.SearchUrl.Contains(searchRequest.Filter.SelectedEmployer) &&
                        request.SearchUrl.Contains(searchRequest.Filter.SelectedCourse) &&
                        request.SearchUrl.Contains(searchRequest.Filter.SelectedStartDate))),
                Times.Once);
        }

        [Test, MoqAutoData]
        public async Task Then_Returns_Mapped_Reservations(
            SearchReservationsRequest request,
            SearchReservationsApiResponse reservationsApiResponse,
            [Frozen] Mock<IApiClient> mockApiClient,
            ReservationService handler)
        {
            var dates = new List<DateTime>
            {
                DateTime.Parse("Apr 2020"), 
                DateTime.Parse("Sep 2010"),
                DateTime.Parse("Aug 2018"),
                DateTime.Parse("Oct 2017"),
                DateTime.Parse("Jul 2019")
            };
            reservationsApiResponse.Filters.StartDateFilters = dates.Select(dt => $"{dt:MMM yyyy} to {dt.AddMonths(3):MMM yyyy}");
            mockApiClient
                .Setup(client => client.Search<SearchReservationsApiResponse>(It.IsAny<ISearchApiRequest>()))
                .ReturnsAsync(reservationsApiResponse);

            var response = await handler.SearchReservations(request);

            response.Reservations.Should().BeEquivalentTo(reservationsApiResponse.Reservations);
            response.TotalReservationsForProvider.Should().Be(reservationsApiResponse.TotalReservationsForProvider);
            response.NumberOfRecordsFound.Should().Be(reservationsApiResponse.NumberOfRecordsFound);
            response.EmployerFilters.Should().BeEquivalentTo(reservationsApiResponse.Filters.EmployerFilters);
            response.CourseFilters.Should().BeEquivalentTo(reservationsApiResponse.Filters.CourseFilters);
            response.StartDateFilters.Should().BeEquivalentTo(reservationsApiResponse.Filters.StartDateFilters);
        }

        [Test, MoqAutoData]
        public async Task Then_Sorts_Filters(
            SearchReservationsRequest request,
            SearchReservationsApiResponse reservationsApiResponse,
            [Frozen] Mock<IApiClient> mockApiClient,
            ReservationService handler)
        {
            var dates = new List<DateTime>
            {
                DateTime.Parse("Apr 2020"), 
                DateTime.Parse("Sep 2010"),
                DateTime.Parse("Aug 2018"),
                DateTime.Parse("Oct 2017"),
                DateTime.Parse("Jul 2019")
            };
            reservationsApiResponse.Filters.StartDateFilters = dates.Select(dt => $"{dt:MMM yyyy} to {dt.AddMonths(3):MMM yyyy}");
            var expectedStartDates = dates
                .OrderBy(dt => dt)
                .Select(dt => $"{dt:MMM yyyy} to {dt.AddMonths(3):MMM yyyy}");
            mockApiClient
                .Setup(client => client.Search<SearchReservationsApiResponse>(It.IsAny<ISearchApiRequest>()))
                .ReturnsAsync(reservationsApiResponse);

            var response = await handler.SearchReservations(request);

            response.EmployerFilters.Should().BeInAscendingOrder();
            response.CourseFilters.Should().BeInAscendingOrder();
            response.StartDateFilters.Should().BeEquivalentTo(expectedStartDates,
                options => options.WithStrictOrdering());
        }
    }
}