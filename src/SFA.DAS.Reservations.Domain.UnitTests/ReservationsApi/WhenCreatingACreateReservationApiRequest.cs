using System;
using AutoFixture.NUnit3;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Domain.ReservationsApi;

namespace SFA.DAS.Reservations.Domain.UnitTests.ReservationsApi
{
    [TestFixture]
    public class WhenCreatingACreateReservationApiRequest
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
            var request = new CreateReservationApiRequest(url, decodeFunc.Object, hashedAccountId, DateTime.Today);
            
            var accountId = request.AccountId;

            decodeFunc.Verify(func => func(hashedAccountId));
            accountId.Should().Be(expectedAccountId);
        }

        [Test, AutoData]
        public void Then_It_Sets_StartDate(
            [Frozen] DateTime startDate,
            CreateReservationApiRequest request)
        {
            request.StartDate.Should().Be(startDate);
        }

        [Test, AutoData]
        public void Then_It_Sets_Url(
            [Frozen] string url,
            CreateReservationApiRequest request)
        {
            request.BaseUrl.Should().Be(url);
        }
    }
}