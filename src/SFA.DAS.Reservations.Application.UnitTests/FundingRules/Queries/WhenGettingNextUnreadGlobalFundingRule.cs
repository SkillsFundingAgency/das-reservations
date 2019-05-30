using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.FundingRules.Queries.GetNextUnreadGlobalFundingRule;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Domain.Rules;
using SFA.DAS.Reservations.Domain.Rules.Api;

namespace SFA.DAS.Reservations.Application.UnitTests.FundingRules.Queries
{
    public class WhenGettingNextUnreadGlobalFundingRules
    {
        private GetNextUnreadGlobalFundingRuleQueryHandler _handler;
        private Mock<IFundingRulesService> _service;
        private GlobalRule _expectedGlobalRule;

        [SetUp]
        public void Arrange()
        {
            _expectedGlobalRule = new GlobalRule
            {
                ActiveFrom = DateTime.Now.AddDays(2)
            };

            var fundingRules = new GetFundingRulesApiResponse
            {
                Rules = new List<ReservationRule>(),
                GlobalRules = new List<GlobalRule>
                {
                    new GlobalRule { ActiveFrom = DateTime.Now.AddDays(-2) },
                    new GlobalRule{ ActiveFrom = DateTime.Now.AddDays(4) },
                    _expectedGlobalRule
                }
            };
            
            _service = new Mock<IFundingRulesService>();
            _service.Setup(s => s.GetUnreadFundingRules(It.IsAny<string>())).ReturnsAsync(fundingRules);

            _handler = new GetNextUnreadGlobalFundingRuleQueryHandler(_service.Object);
        }

        [Test]
        public async Task Then_The_Next_Active_Funding_Rule_Is_Returned()
        {
            //Act
            var actual = await _handler.Handle(new GetNextUnreadGlobalFundingRuleQuery{Id = "123"}, new CancellationToken());

           //Assert
           Assert.AreEqual(_expectedGlobalRule, actual.Rule);
        }

        [Test]
        public async Task Then_The_User_Id_Is_Used_To_Filter_Results()
        {
            //Act
            var expectedUserId = "123";
            var actual = await _handler.Handle(new GetNextUnreadGlobalFundingRuleQuery{Id = expectedUserId}, new CancellationToken());

            //Assert
            _service.Verify(s => s.GetUnreadFundingRules(expectedUserId), Times.Once);
        }

        [Test]
        public void Then_Throws_Exception_If_One_Occurs()
        {
            //Arrange
            var expectedException = new Exception();
            _service.Setup(s => s.GetUnreadFundingRules(It.IsAny<string>())).Throws(expectedException);

            //Act
            var actualException = Assert.ThrowsAsync<Exception>(() => 
                _handler.Handle(new GetNextUnreadGlobalFundingRuleQuery{Id = "123"}, new CancellationToken()));

            //Assert
            Assert.AreEqual(expectedException, actualException);
        }

        [Test]
        public async Task Then_If_No_Active_Rules_Are_Found_Null_Is_Returned()
        {
            //Arrange
            var fundingRules = new GetFundingRulesApiResponse
            {
                Rules = new List<ReservationRule>(),
                GlobalRules = new List<GlobalRule>()
            };

            _service.Setup(s => s.GetUnreadFundingRules(It.IsAny<string>())).ReturnsAsync(fundingRules);

            //Act
            var actual = await _handler.Handle(new GetNextUnreadGlobalFundingRuleQuery{Id = "123"}, new CancellationToken());

            //Assert
            Assert.IsNull(actual.Rule);
        }

        [Test]
        public async Task Then_If_No_Active_Rules_Are_Found_In_Future_A_Null_Is_Returned()
        {
            //Arrange
            var fundingRules = new GetFundingRulesApiResponse
            {
                Rules = new List<ReservationRule>(),
                GlobalRules = new List<GlobalRule>
                {
                    new GlobalRule { ActiveFrom = DateTime.Now.AddDays(-2) }
                }
            };

            _service.Setup(s => s.GetUnreadFundingRules(It.IsAny<string>())).ReturnsAsync(fundingRules);

            //Act
            var actual = await _handler.Handle(new GetNextUnreadGlobalFundingRuleQuery{Id = "123"}, new CancellationToken());

            //Assert
            Assert.IsNull(actual.Rule);
        }
    }
}
