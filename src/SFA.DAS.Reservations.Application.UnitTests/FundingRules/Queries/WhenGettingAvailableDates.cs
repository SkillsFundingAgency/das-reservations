using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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
        private GetAvailableDatesQueryHandler _handler;
        private Mock<IFundingRulesService> _service;
        private GetAvailableDatesApiResponse _expectedAvailableDates;

        [SetUp]
        public void Arrange()
        {
            _expectedAvailableDates = new GetAvailableDatesApiResponse
            {
                AvailableDates = new List<DateTime>()
            };

            _service = new Mock<IFundingRulesService>();
            _service.Setup(s => s.GetAvailableDates()).ReturnsAsync(_expectedAvailableDates);

            _handler = new GetAvailableDatesQueryHandler(_service.Object);
        }

        [Test]
        public async Task Then_The_AvailableDates_Are_Returned()
        {
            //Act
            var actual = await _handler.Handle(new GetAvailableDatesQuery(), new CancellationToken());

            //Assert
            actual.AvailableDates.Should().BeEquivalentTo(_expectedAvailableDates.AvailableDates);
        }

        [Test]
        public void Then_Throws_Exception_If_One_Occurs()
        {
            //Arrange
            var expectedException = new Exception();
            _service.Setup(s => s.GetAvailableDates()).Throws(expectedException);

            //Act
            var actualException = Assert.ThrowsAsync<Exception>(() =>
                _handler.Handle(new GetAvailableDatesQuery(), new CancellationToken()));

            //Assert
            Assert.AreEqual(expectedException, actualException);
        }
    }
}
