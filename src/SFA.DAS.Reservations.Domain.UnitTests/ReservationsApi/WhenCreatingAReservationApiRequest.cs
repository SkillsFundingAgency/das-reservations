using System;
using AutoFixture.NUnit3;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Domain.Reservations.Api;

namespace SFA.DAS.Reservations.Domain.UnitTests.ReservationsApi
{
    [TestFixture]
    public class WhenCreatingAReservationApiRequest
    {

        [Test, MoqAutoData]
        public void Then_It_Sets_And_Decodes_AccountId(
            string url,
            long expectedAccountId,
            [Frozen] string hashedAccountId,
            [Frozen] Mock<Func<string, long>> decodeFunc)
        {
            decodeFunc
                .Setup(func => func(hashedAccountId))
                .Returns(expectedAccountId);

            // note: I've had to construct here as using autodata doesn't inject the mock func for some reason
            var request = new ReservationApiRequest(url, decodeFunc.Object, hashedAccountId, DateTime.Today, Guid.NewGuid());
            
            var accountId = request.AccountId;

            decodeFunc.Verify(func => func(hashedAccountId));
            accountId.Should().Be(expectedAccountId);
        }

        [Test, AutoData]
        public void Then_It_Sets_StartDate(
            [Frozen] DateTime startDate,
            ReservationApiRequest request)
        {
            request.StartDate.Should().Be(startDate);
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
            var decode = new Mock<Func<string, long>>();
            decode.Setup(func => func("ABC34r")).Returns(123);
            var request = new ReservationApiRequest("http://test/", decode.Object,"ABC34r",DateTime.Today, expectedId);

            request.GetUrl.Should().Be($"http://test/api/accounts/123/reservations/{expectedId}");
        }
    }
}