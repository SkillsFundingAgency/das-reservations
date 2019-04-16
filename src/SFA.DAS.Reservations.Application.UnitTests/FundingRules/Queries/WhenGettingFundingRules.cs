using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.FundingRules.Queries.GetFundingRules;
using SFA.DAS.Reservations.Application.FundingRules.Services;
using SFA.DAS.Reservations.Domain.Rules;
using SFA.DAS.Reservations.Domain.Rules.Api;

namespace SFA.DAS.Reservations.Application.UnitTests.FundingRules.Queries
{
    public class WhenGettingFundingRules
    {
        private GetFundingRulesQueryHandler _handler;
        private Mock<IFundingRulesService> _service;
        private GetFundingRulesApiResponse _expectedFundingRules;

        [SetUp]
        public void Arrange()
        {
            _expectedFundingRules = new GetFundingRulesApiResponse
            {
                CourseRules = new List<ReservationRule>(),
                GlobalRules = new List<GlobalRule>()
            };
            
            _service = new Mock<IFundingRulesService>();
            _service.Setup(s => s.GetFundingRules()).ReturnsAsync(_expectedFundingRules);

            _handler = new GetFundingRulesQueryHandler(_service.Object);
        }

        [Test]
        public async Task Then_The_Courses_Are_Returned()
        {
            //Act
            var actual = await _handler.Handle(new GetFundingRulesQuery(), new CancellationToken());

            //Assert
            actual.FundingRules.Should().BeEquivalentTo(_expectedFundingRules);
        }

        [Test]
        public void Then_Throws_Exception_If_One_Occurs()
        {
            //Arrange
            var expectedException = new Exception();
            _service.Setup(s => s.GetFundingRules()).Throws(expectedException);

            //Act
            var actualException = Assert.ThrowsAsync<Exception>(() => 
                _handler.Handle(new GetFundingRulesQuery(), new CancellationToken()));

            //Assert
            Assert.AreEqual(expectedException, actualException);
        }

    }
}
