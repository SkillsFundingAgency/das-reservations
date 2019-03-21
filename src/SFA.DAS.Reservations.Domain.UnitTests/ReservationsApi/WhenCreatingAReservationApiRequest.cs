using System;
using AutoFixture.NUnit3;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Reservations.Domain.Reservations.Api;

namespace SFA.DAS.Reservations.Domain.UnitTests.ReservationsApi
{
    [TestFixture]
    public class WhenCreatingAReservationApiRequest
    {
        [Test, AutoData]
        public void Then_It_Sets_Id(
            [Frozen] Guid id,
            ReservationApiRequest request)
        {
            request.Id.Should().Be(id);
        }

        [Test, AutoData]
        public void Then_It_Sets_StartDate()
        {
            var startDate = DateTime.Now.AddDays(-10);
            var request = new ReservationApiRequest("test", 1, startDate, Guid.NewGuid(), "Test Name");

            request.StartDate.Should().Be(startDate.ToString("yyyy-MMM-dd"));
        }

        [Test, AutoData]
        public void Then_It_Sets_CreateUrl(
            [Frozen] string url,
            ReservationApiRequest request)
        {
            request.CreateUrl.Should().Be($"{url}api/accounts/{request.AccountId}/reservations");
        }

        [Test]
        public void Then_It_Sets_The_GetUrl()
        {
            var expectedId = Guid.NewGuid();
            
            var request = new ReservationApiRequest("http://test/", expectedId);

            request.GetUrl.Should().Be($"http://test/api/reservations/{expectedId}");
        }
    }
}