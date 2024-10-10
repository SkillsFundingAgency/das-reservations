using System;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.FundingRules.Queries.GetAvailableDates;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Domain.Rules.Api;

namespace SFA.DAS.Reservations.Application.UnitTests.FundingRules.Queries
{
    public class WhenGettingAvailableDates
    {
        private GetAvailableDatesQuery _query;
        private GetAvailableDatesQueryHandler _handler;
        private Mock<IReservationsOuterService> _service;
        private GetAvailableDatesApiResponse _expectedApiResponse;

        [SetUp]
        public void Arrange()
        {
            var fixture = new Fixture();
            _query = fixture.Create<GetAvailableDatesQuery>();
            _expectedApiResponse = fixture.Create<GetAvailableDatesApiResponse>();

            _service = new Mock<IReservationsOuterService>();
            _service
                .Setup(s => s.GetAvailableDates(_query.AccountLegalEntityId))
                .ReturnsAsync(_expectedApiResponse);

            _handler = new GetAvailableDatesQueryHandler(_service.Object);
        }

        [Test]
        public async Task Then_The_AvailableDates_Are_Returned()
        {
            //Act
            var actual = await _handler.Handle(_query, new CancellationToken());

            //Assert
            actual.AvailableDates.Should().BeEquivalentTo(_expectedApiResponse.AvailableDates);
        }

        [Test]
        public async Task Then_The_PreviousMonth_Is_Returned()
        {
            //Act
            var actual = await _handler.Handle(_query, new CancellationToken());

            //Assert
            actual.Should().BeEquivalentTo(_expectedApiResponse);
        }

        [Test]
        public void Then_Throws_Exception_If_One_Occurs()
        {
            //Arrange
            var expectedException = new Exception();
            _service
                .Setup(s => s.GetAvailableDates(It.IsAny<long>()))
                .Throws(expectedException);

            //Act
            var actualException = Assert.ThrowsAsync<Exception>(() =>
                _handler.Handle(new GetAvailableDatesQuery(), new CancellationToken()));

            //Assert
            actualException.Should().Be(expectedException);
        }
    }
}
