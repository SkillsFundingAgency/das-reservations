﻿using System;
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

        [Test]
        public void Then_It_Sets_Values()
        {
            var startDate = DateTime.Now.AddDays(-10);
            var courseId = "1-2-3";
            var userId = Guid.NewGuid();
            var request = new ReservationApiRequest("test", 1, 2, startDate, Guid.NewGuid(),1, "Test Name",courseId, userId);

            request.StartDate.Should().Be(startDate.ToString("yyyy-MMM-dd"));
            request.CourseId.Should().Be(courseId);
            request.UserId.Should().Be(userId);
        }

        [Test, AutoData]
        public void Then_It_Sets_CreateUrl(
            [Frozen] string url,
            ReservationApiRequest request)
        {
            request.CreateUrl.Should().Be($"{url}api/accounts/{request.AccountId}/reservations");
        }

        [Test, AutoData]
        public void Then_It_Sets_The_GetUrl(
            string url,
            Guid expectedId)
        {
            var request = new ReservationApiRequest(url, expectedId);

            request.GetUrl.Should().Be($"{url}api/reservations/{expectedId}");
        }

        [Test, AutoData]
        public void Then_It_Sets_The_GetAllUrl(
            string url,
            long accountId)
        {
            var request = new ReservationApiRequest(url, accountId);

            request.GetAllUrl.Should().Be($"{url}api/accounts/{accountId}/reservations");
        }

        [Test, AutoData]
        public void Then_It_Sets_The_DeleteUrl(
            string url,
            bool employerDeleted,
            Guid reservationId)
        {
            var request = new ReservationApiRequest(url, reservationId, employerDeleted);

            request.DeleteUrl.Should().Be($"{url}api/reservations/{reservationId}?employerDeleted={employerDeleted}");
        }
    }
}