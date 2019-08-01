using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.Employers.Queries;
using SFA.DAS.Reservations.Application.Reservations.Commands.CacheReservationEmployer;
using SFA.DAS.Reservations.Domain.Employers;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Domain.Rules;
using SFA.DAS.Reservations.Domain.Rules.Api;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Reservations.Application.UnitTests.Reservations.Commands.CacheReservationEmployer
{
    [TestFixture]
    public class WhenValidatingACacheReservationEmployerCommand
    {
        [Test, MoqAutoData]
        public async Task And_No_Id_Then_Invalid(
            CacheReservationEmployerCommand command,
            [Frozen]Mock<IFundingRulesService> rulesService,
            CacheReservationEmployerCommandValidator validator)
        {
            ConfigureRulesServiceWithNoGlobalRules(rulesService);
            command.Id = Guid.Empty;

            var result = await validator.ValidateAsync(command);

            result.IsValid().Should().BeFalse();
            result.ValidationDictionary.Count.Should().Be(1);
            result.ValidationDictionary
                .Should().ContainKey(nameof(CacheReservationEmployerCommand.Id))
                .WhichValue.Should().Be($"{nameof(CacheReservationEmployerCommand.Id)} has not been supplied");
        }

        [Test, MoqAutoData]
        public async Task Then_The_Account_Is_Checked_Against_The_AccountRules_And_An_Error_Returned_If_Not_Valid(
            CacheReservationEmployerCommand command,
            [Frozen]Mock<IFundingRulesService> rulesService,
            CacheReservationEmployerCommandValidator validator)
        {
            rulesService.Setup(x => x.GetAccountFundingRules(command.AccountId)).ReturnsAsync(
                new GetAccountFundingRulesApiResponse
                {
                    GlobalRules = new List<GlobalRule> {new GlobalRule
                    {
                        Restriction = AccountRestriction.Account,
                        RuleType = GlobalRuleType.ReservationLimit
                    }}
                });

            var result = await validator.ValidateAsync(command);

            result.IsValid().Should().BeTrue();
            result.FailedRuleValidation.Should().BeTrue();
        }

        [Test, MoqAutoData]
        public async Task Then_The_GlobalRules_Are_Checked_And_An_Error_Returned_If_Not_Valid(
            CacheReservationEmployerCommand command,
            [Frozen]Mock<IFundingRulesService> rulesService,
            CacheReservationEmployerCommandValidator validator)
        {
            rulesService.Setup(x => x.GetFundingRules()).ReturnsAsync(new GetFundingRulesApiResponse
            {
                GlobalRules = new List<GlobalRule>
                {
                    new GlobalRule
                    {
                        Restriction = AccountRestriction.NonLevy,
                        RuleType = GlobalRuleType.FundingPaused
                    }
                }
            });

            var result = await validator.ValidateAsync(command);

            result.IsValid().Should().BeTrue();
            result.FailedGlobalRuleValidation.Should().BeTrue();
        }

        [Test, MoqAutoData]
        public async Task And_AccountId_Less_Than_One_Then_Invalid(
            CacheReservationEmployerCommand command, 
            [Frozen]Mock<IFundingRulesService> rulesService,
            CacheReservationEmployerCommandValidator validator)
        {
            ConfigureRulesServiceWithNoGlobalRules(rulesService);
            command.AccountId = default(long);

            var result = await validator.ValidateAsync(command);

            result.IsValid().Should().BeFalse();
            result.ValidationDictionary.Count.Should().Be(1);
            result.ValidationDictionary
                .Should().ContainKey(nameof(CacheReservationEmployerCommand.AccountId))
                .WhichValue.Should().Be($"{nameof(CacheReservationEmployerCommand.AccountId)} has not been supplied");
        }

        [Test, MoqAutoData]
        public async Task And_AccountLegalEntityId_Less_Than_One_Then_Invalid(
            CacheReservationEmployerCommand command,
            [Frozen]Mock<IFundingRulesService> rulesService,
            CacheReservationEmployerCommandValidator validator)
        {
            ConfigureRulesServiceWithNoGlobalRules(rulesService);
            command.AccountLegalEntityId = default(long);

            var result = await validator.ValidateAsync(command);

            result.IsValid().Should().BeFalse();
            result.ValidationDictionary.Count.Should().Be(1);
            result.ValidationDictionary
                .Should().ContainKey(nameof(CacheReservationEmployerCommand.AccountLegalEntityId))
                .WhichValue.Should().Be($"{nameof(CacheReservationEmployerCommand.AccountLegalEntityId)} has not been supplied");
        }

        [Test, MoqAutoData]
        public async Task And_AccountLegalEntityName_Null_Then_Invalid(
            CacheReservationEmployerCommand command,
            [Frozen]Mock<IFundingRulesService> rulesService,
            CacheReservationEmployerCommandValidator validator)
        {
            ConfigureRulesServiceWithNoGlobalRules(rulesService);
            command.AccountLegalEntityName = null;

            var result = await validator.ValidateAsync(command);

            result.IsValid().Should().BeFalse();
            result.ValidationDictionary.Count.Should().Be(1);
            result.ValidationDictionary
                .Should().ContainKey(nameof(CacheReservationEmployerCommand.AccountLegalEntityName))
                .WhichValue.Should().Be($"{nameof(CacheReservationEmployerCommand.AccountLegalEntityName)} has not been supplied");
        }

        [Test, MoqAutoData]
        public async Task And_AccountLegalEntityName_Whitespace_Then_Invalid(
            CacheReservationEmployerCommand command,
            [Frozen]Mock<IFundingRulesService> rulesService,
            CacheReservationEmployerCommandValidator validator)
        {
            ConfigureRulesServiceWithNoGlobalRules(rulesService);
            command.AccountLegalEntityName = " ";

            var result = await validator.ValidateAsync(command);

            result.IsValid().Should().BeFalse();
            result.ValidationDictionary.Count.Should().Be(1);
            result.ValidationDictionary
                .Should().ContainKey(nameof(CacheReservationEmployerCommand.AccountLegalEntityName))
                .WhichValue.Should().Be($"{nameof(CacheReservationEmployerCommand.AccountLegalEntityName)} has not been supplied");
        }

        [Test, MoqAutoData]
        public async Task And_AccountLegalEntityPublicHashedId_Null_Then_Invalid(
            CacheReservationEmployerCommand command,
            [Frozen]Mock<IFundingRulesService> rulesService,
            CacheReservationEmployerCommandValidator validator)
        {
            ConfigureRulesServiceWithNoGlobalRules(rulesService);
            command.AccountLegalEntityPublicHashedId = null;

            var result = await validator.ValidateAsync(command);

            result.IsValid().Should().BeFalse();
            result.ValidationDictionary.Count.Should().Be(1);
            result.ValidationDictionary
                .Should().ContainKey(nameof(CacheReservationEmployerCommand.AccountLegalEntityPublicHashedId))
                .WhichValue.Should().Be($"{nameof(CacheReservationEmployerCommand.AccountLegalEntityPublicHashedId)} has not been supplied");
        }

        [Test, MoqAutoData]
        public async Task And_AccountLegalEntityPublicHashedId_Whitespace_Then_Invalid(
            CacheReservationEmployerCommand command,
            [Frozen]Mock<IFundingRulesService> rulesService,
            CacheReservationEmployerCommandValidator validator)
        {
            ConfigureRulesServiceWithNoGlobalRules(rulesService);
            command.AccountLegalEntityPublicHashedId = " ";

            var result = await validator.ValidateAsync(command);

            result.IsValid().Should().BeFalse();
            result.ValidationDictionary.Count.Should().Be(1);
            result.ValidationDictionary
                .Should().ContainKey(nameof(CacheReservationEmployerCommand.AccountLegalEntityPublicHashedId))
                .WhichValue.Should().Be($"{nameof(CacheReservationEmployerCommand.AccountLegalEntityPublicHashedId)} has not been supplied");
        }

        [Test, MoqAutoData]
        public async Task And_Provider_Is_Not_Trusted_For_Account_Legal_Entity_Then_Mark_As_Not_Authorised(
            CacheReservationEmployerCommand command,
            [Frozen]Mock<IFundingRulesService> rulesService,
            [Frozen] Mock<IMediator> mediator,
            [Frozen]GetTrustedEmployersResponse response,
            CacheReservationEmployerCommandValidator validator)
        {
            mediator.Setup(m => m.Send(It.IsAny<GetTrustedEmployersQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            var result = await validator.ValidateAsync(command);

            result.IsValid().Should().BeTrue();
            result.FailedAuthorisationValidation.Should().BeTrue();
        }

        [Test, MoqAutoData]
        public async Task And_Provider_Is__Trusted_For_Account_Legal_Entity_Then_Mark_As_Authorised(
            CacheReservationEmployerCommand command,
            [Frozen]Mock<IFundingRulesService> rulesService,
            [Frozen] Mock<IMediator> mediator,
            [Frozen]GetTrustedEmployersResponse response,
            CacheReservationEmployerCommandValidator validator)
        {
            response.Employers = new[]
            {
                new Employer()
                {
                    AccountLegalEntityPublicHashedId = command.AccountLegalEntityPublicHashedId
                }
            };

            mediator.Setup(m => m.Send(It.IsAny<GetTrustedEmployersQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            var result = await validator.ValidateAsync(command);

            result.IsValid().Should().BeTrue();
            result.FailedAuthorisationValidation.Should().BeFalse();
        }

        [Test, MoqAutoData]
        public async Task And_All_Properties_Ok_Then_Valid(
            CacheReservationEmployerCommand command,
            [Frozen]Mock<IFundingRulesService> rulesService,
            CacheReservationEmployerCommandValidator validator)
        {
            ConfigureRulesServiceWithNoGlobalRules(rulesService);
            var result = await validator.ValidateAsync(command);

            result.IsValid().Should().BeTrue();
            result.FailedRuleValidation.Should().BeFalse();
        }

        private static void ConfigureRulesServiceWithNoGlobalRules(Mock<IFundingRulesService> rulesService)
        {
            rulesService.Setup(x => x.GetAccountFundingRules(It.IsAny<long>())).ReturnsAsync(
                new GetAccountFundingRulesApiResponse
                {
                    GlobalRules = new List<GlobalRule> { null }
                });
        }
    }
}