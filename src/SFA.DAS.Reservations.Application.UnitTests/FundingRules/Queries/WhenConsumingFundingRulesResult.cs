using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
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
            result.AccountRules.Count.Should().Be(1);
            result.ActiveAccountRules.Count().Should().Be(1);

            var activeRule = result.ActiveAccountRules.First();
            activeRule.Should().Be(expectedActiveRule);
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
            result.GlobalRules.Count.Should().Be(1);
            result.ActiveGlobalRules.Count().Should().Be(1);

            var activeRule = result.ActiveGlobalRules.First();
            activeRule.Should().Be(expectedGlobalRule);
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
            result.AccountRules.Count.Should().Be(1);
            result.ActiveAccountRules.Should().BeEmpty();
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
            result.GlobalRules.Count.Should().Be(1);
            result.ActiveGlobalRules.Should().BeEmpty();
        }
    }
}
