using NUnit.Framework;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using SFA.DAS.Reservations.Application.FundingRules.Queries.GetAccountFundingRules;
using SFA.DAS.Reservations.Domain.Interfaces;
using Moq;
using SFA.DAS.Reservations.Application.Validation;
using SFA.DAS.Reservations.Domain.Rules;
using SFA.DAS.Reservations.Domain.Rules.Api;
using SFA.DAS.Testing.AutoFixture;
using ValidationResult = SFA.DAS.Reservations.Application.Validation.ValidationResult;

namespace SFA.DAS.Reservations.Application.UnitTests.FundingRules.Queries
{
    public class WhenGettingAccountFundingRules
    {
        private GetAccountFundingRulesQueryHandler _handler;
        private Mock<IFundingRulesService> _fundingRulesService;
        private Mock<IValidator<GetAccountFundingRulesQuery>> _validator;
        private GetAccountFundingRulesQuery _query;

        [SetUp]
        public void Arrange()
        {
            _fundingRulesService = new Mock<IFundingRulesService>();
            _validator = new Mock<IValidator<GetAccountFundingRulesQuery>>();

        }

        [Test, MoqAutoData]
        public async Task WhenRulesAreValid_ThenTheRulesAreReturned(
            [Frozen] long accountId)
        {
            //Arrange
            var expectedRules = new GetAccountFundingRulesApiResponse()
            {
                GlobalRules = new List<GlobalRule>()
                {
                    new GlobalRule()
                    {
                        Id = accountId,
                        RuleType = GlobalRuleType.ReservationLimit,
                    }
                }

            };
            _query = new GetAccountFundingRulesQuery(){AccountId = accountId};

            _validator.Setup(m => m.ValidateAsync(_query))
                .ReturnsAsync(new ValidationResult());

            _fundingRulesService.Setup(m => m.GetAccountFundingRules(accountId)).ReturnsAsync(expectedRules);
            _handler = new GetAccountFundingRulesQueryHandler(_fundingRulesService.Object,_validator.Object);

            //Act
            var result =  await _handler.Handle(_query, CancellationToken.None);

            //Assert
            result.AccountFundingRules.Should().Be(expectedRules);
            result.AccountFundingRules.GlobalRules.First().Should().Be(expectedRules.GlobalRules.First());

        }

        [Test, MoqAutoData]
        public void WhenRulesAreNotValid_ThenValidationExcetionIsThrown(
            [Frozen] long accountId)
        {
            //Arrange
            var expectedRules = new GetAccountFundingRulesApiResponse()
            {
                GlobalRules = new List<GlobalRule>()
                {
                    new GlobalRule()
                    {
                        Id = accountId,
                        RuleType = GlobalRuleType.ReservationLimit,
                    }
                }

            };
            _query = new GetAccountFundingRulesQuery() {AccountId = accountId};

            _validator.Setup(m => m.ValidateAsync(_query)).ReturnsAsync(new ValidationResult
            {
                ValidationDictionary = new Dictionary<string, string>()
                {
                    ["Error"] = "Error"
                }
            });

            _fundingRulesService.Setup(m => m.GetAccountFundingRules(accountId)).ReturnsAsync(expectedRules);
            _handler = new GetAccountFundingRulesQueryHandler(_fundingRulesService.Object, _validator.Object);

            //Assert + Act
            Assert.ThrowsAsync<ValidationException>(async () => 
                await _handler.Handle(_query, new CancellationToken()));
        }

        [Test, MoqAutoData]
        public async Task IfGlobalRulesReturned_ThenSetsActiveRulePropertyInResult(
            [Frozen] long accountId)

        {
            //Arrange
            var expectedRules = new GetAccountFundingRulesApiResponse()
            {
                GlobalRules = new List<GlobalRule>()
                {
                    new GlobalRule
                    {
                        Id = accountId,
                        RuleType = GlobalRuleType.ReservationLimit,
                    }
                }

            };
            _query = new GetAccountFundingRulesQuery { AccountId = accountId };

            _validator.Setup(m => m.ValidateAsync(_query))
                .ReturnsAsync(new ValidationResult());

            _fundingRulesService.Setup(m => m.GetAccountFundingRules(accountId)).ReturnsAsync(expectedRules);
            _handler = new GetAccountFundingRulesQueryHandler(_fundingRulesService.Object, _validator.Object);

            //Act
            var result = await _handler.Handle(_query, CancellationToken.None);

            //Assert
            result.ActiveRule.RuleType.Should().Be(expectedRules.GlobalRules.First().RuleType);
        }


        [Test, MoqAutoData]
        public async Task If_Multiple_GlobalRulesReturned_ThenSetsActiveRulePropertyInResult(
            [Frozen] long accountId)

        {
            //Arrange
            var expectedRules = new GetAccountFundingRulesApiResponse()
            {
                GlobalRules = new List<GlobalRule>()
                {
                    null,
                    new GlobalRule
                    {
                        Id = accountId,
                        RuleType = GlobalRuleType.ReservationLimit,
                    }
                }

            };
            _query = new GetAccountFundingRulesQuery { AccountId = accountId };

            _validator.Setup(m => m.ValidateAsync(_query))
                .ReturnsAsync(new ValidationResult());

            _fundingRulesService.Setup(m => m.GetAccountFundingRules(accountId)).ReturnsAsync(expectedRules);
            _handler = new GetAccountFundingRulesQueryHandler(_fundingRulesService.Object, _validator.Object);

            //Act
            var result = await _handler.Handle(_query, CancellationToken.None);

            //Assert
            result.ActiveRule.RuleType.Should().Be(expectedRules.GlobalRules.First(x => x != null).RuleType);
        }

        [Test, MoqAutoData]
        public async Task If_Multiple_GlobalRulesReturned_ThenSetsTheEarliestStartingRuleAsActive(
            [Frozen] long accountId)

        {
            //Arrange
            var expectedRules = new GetAccountFundingRulesApiResponse()
            {
                GlobalRules = new List<GlobalRule>()
                {
                    new GlobalRule
                    {
                        Id = accountId,
                        ActiveFrom = System.DateTime.Now.AddDays(-1),
                        RuleType = GlobalRuleType.ReservationLimit,
                    },
                    new GlobalRule
                    {
                        Id = accountId,
                        ActiveFrom = System.DateTime.Now.AddDays(-20),
                        RuleType = GlobalRuleType.DynamicPause,
                    }
                }

            };
            _query = new GetAccountFundingRulesQuery { AccountId = accountId };

            _validator.Setup(m => m.ValidateAsync(_query))
                .ReturnsAsync(new ValidationResult());

            _fundingRulesService.Setup(m => m.GetAccountFundingRules(accountId)).ReturnsAsync(expectedRules);
            _handler = new GetAccountFundingRulesQueryHandler(_fundingRulesService.Object, _validator.Object);

            //Act
            var result = await _handler.Handle(_query, CancellationToken.None);

            //Assert
            result.ActiveRule.RuleType.Should().Be(GlobalRuleType.DynamicPause);
        }

        [Test, MoqAutoData]
        public async Task IfNoGlobalRulesReturned_ThenSetsActiveRulePropertyToNull(
            [Frozen] long accountId)

        {
            //Arrange
            var expectedRules = new GetAccountFundingRulesApiResponse { 
                GlobalRules = new List<GlobalRule>()};

            _query = new GetAccountFundingRulesQuery { AccountId = accountId };

            _validator.Setup(m => m.ValidateAsync(_query))
                .ReturnsAsync(new ValidationResult());

            _fundingRulesService.Setup(m => m.GetAccountFundingRules(accountId)).ReturnsAsync(expectedRules);
            _handler = new GetAccountFundingRulesQueryHandler(_fundingRulesService.Object, _validator.Object);

            //Act
            var result = await _handler.Handle(_query, CancellationToken.None);

            //Assert
            result.ActiveRule.Should().BeNull();
        }
    }
}
