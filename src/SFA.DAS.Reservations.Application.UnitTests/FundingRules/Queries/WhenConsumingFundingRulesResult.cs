using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.FundingRules.Queries.GetFundingRules;
using SFA.DAS.Reservations.Domain.Rules;

namespace SFA.DAS.Reservations.Application.UnitTests.FundingRules.Queries
{
    public class WhenConsumingFundingRulesResult
    {

        [Test]
        public void ThenWillHaveActiveAccountRules()
        {
            //Arrange
            var result = new GetFundingRulesResult
            {
                GlobalRules = new List<GlobalRule>(),
                AccountRules = new List<ReservationRule>()
            };

            var expectedActiveRule = new ReservationRule
            {
                ActiveFrom = DateTime.Now.AddDays(-2),
                ActiveTo = DateTime.Now.AddDays(2)
            };

            //Act
            result.AccountRules.Add(expectedActiveRule);

            //Assert
            Assert.AreEqual(1, result.AccountRules.Count);
            Assert.AreEqual(1, result.ActiveAccountRules.Count());

            var activeRule = result.ActiveAccountRules.First();
            Assert.AreEqual(expectedActiveRule, activeRule);
        }

        [Test]
        public void ThenWillHaveActiveGlobalRules()
        {
            //Arrange
            var result = new GetFundingRulesResult
            {
                GlobalRules = new List<GlobalRule>(),
                AccountRules = new List<ReservationRule>()
            };

            var expectedGlobalRule = new GlobalRule
            {
                ActiveFrom = DateTime.Now.AddDays(-2)
            };

            result.GlobalRules.Add(expectedGlobalRule);

            //Assert
            Assert.AreEqual(1, result.GlobalRules.Count);
            Assert.AreEqual(1, result.ActiveGlobalRules.Count());

            var activeRule = result.ActiveGlobalRules.First();
            Assert.AreEqual(expectedGlobalRule, activeRule);
        }

        [Test]
        public void ThenWillHaveNoActiveAccountRules()
        {
            //Arrange
            var result = new GetFundingRulesResult
            {
                GlobalRules = new List<GlobalRule>(),
                AccountRules = new List<ReservationRule>()
            };

            var expectedActiveRule = new ReservationRule
            {
                ActiveFrom = DateTime.Now.AddDays(20),
                ActiveTo = DateTime.Now.AddDays(30)
            };

            //Act
            result.AccountRules.Add(expectedActiveRule);

            //Assert
            Assert.AreEqual(1, result.AccountRules.Count);
            Assert.IsEmpty(result.ActiveAccountRules);
        }

        
        [Test]
        public void ThenWillHaveNoActiveGlobalRules()
        {
            //Arrange
            var result = new GetFundingRulesResult
            {
                GlobalRules = new List<GlobalRule>(),
                AccountRules = new List<ReservationRule>()
            };

            var expectedGlobalRule = new GlobalRule
            {
                ActiveFrom = DateTime.Now.AddDays(2)
            };

            result.GlobalRules.Add(expectedGlobalRule);

            //Assert
            Assert.AreEqual(1, result.GlobalRules.Count);
            Assert.IsEmpty(result.ActiveGlobalRules);
        }
    }
}
