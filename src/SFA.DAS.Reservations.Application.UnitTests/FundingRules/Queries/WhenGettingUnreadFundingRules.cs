using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.FundingRules.Queries.GetUnreadFundingRules;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Domain.Rules;
using SFA.DAS.Reservations.Domain.Rules.Api;

namespace SFA.DAS.Reservations.Application.UnitTests.FundingRules.Queries
{
    public class WhenGettingUnreadFundingRules
    {
        private GetUnreadFundingRulesQueryHandler _handler;
        private Mock<IFundingRulesService> _service;
        private GetFundingRulesApiResponse _expectedFundingRules;

        [SetUp]
        public void Arrange()
        {
            _expectedFundingRules = new GetFundingRulesApiResponse
            {
                Rules = new List<ReservationRule>(),
                GlobalRules = new List<GlobalRule>()
            };
            
            _service = new Mock<IFundingRulesService>();
            _service.Setup(s => s.GetUnreadFundingRules(It.IsAny<string>())).ReturnsAsync(_expectedFundingRules);

            _handler = new GetUnreadFundingRulesQueryHandler(_service.Object);
        }

        [Test]
        public async Task Then_The_Unread_FundingRules_Are_Returned()
        {
            //Act
            var actual = await _handler.Handle(new GetUnreadFundingRulesQuery(), new CancellationToken());

            //Assert
            actual.FundingRules.Should().BeEquivalentTo(_expectedFundingRules);
        }

        [Test]
        public async Task Then_The_User_Id_Is_Used_To_Help_Filter_Rules()
        {
            //Act
            var userId = "123";
            await _handler.Handle(new GetUnreadFundingRulesQuery{Id = userId}, new CancellationToken());

            //Assert
            _service.Verify(s => s.GetUnreadFundingRules(userId), Times.Once);
        }

        [Test]
        public void Then_Throws_Exception_If_One_Occurs()
        {
            //Arrange
            var expectedException = new Exception();
            _service.Setup(s => s.GetUnreadFundingRules(It.IsAny<string>())).Throws(expectedException);

            //Act
            var actualException = Assert.ThrowsAsync<Exception>(() => 
                _handler.Handle(new GetUnreadFundingRulesQuery(), new CancellationToken()));

            //Assert
            Assert.AreEqual(expectedException, actualException);
        }
    }
}
