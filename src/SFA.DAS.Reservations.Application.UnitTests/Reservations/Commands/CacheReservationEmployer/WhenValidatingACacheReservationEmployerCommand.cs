using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.NUnit3;
using FluentAssertions;
using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.Employers.Queries.GetLegalEntities;
using SFA.DAS.Reservations.Application.Providers.Queries.GetTrustedEmployers;
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
            [Frozen] Mock<IMediator> mockMediator,
            [Frozen]Mock<IFundingRulesService> rulesService,
            CacheReservationEmployerCommandValidator validator)
        {
            SetupAccountLegalEntityAsNonLevy(mockMediator);
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
            [Frozen] Mock<IMediator> mockMediator,
            [Frozen]Mock<IFundingRulesService> rulesService,
            CacheReservationEmployerCommandValidator validator)
        {
            SetupAccountLegalEntityAsNonLevy(mockMediator);
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
            [Frozen] Mock<IMediator> mockMediator,
            [Frozen]Mock<IFundingRulesService> rulesService,
            CacheReservationEmployerCommandValidator validator)
        {
            SetupAccountLegalEntityAsNonLevy(mockMediator);
            rulesService.Setup(x => x.GetFundingRules()).ReturnsAsync(new GetFundingRulesApiResponse
            {
                GlobalRules = new List<GlobalRule>
                {
                    new GlobalRule
                    {
                        Restriction = AccountRestriction.NonLevy,
                        RuleType = GlobalRuleType.FundingPaused,
                        ActiveFrom = DateTime.UtcNow.AddMonths(-1)
                    }
                }
            });

            var result = await validator.ValidateAsync(command);

            result.IsValid().Should().BeTrue();
            result.FailedGlobalRuleValidation.Should().BeTrue();
        }

        [Test, MoqAutoData]
        public async Task Then_The_GlobalRules_Are_Checked_And_If_There_Is_An_Upcoming_Restriction_An_Error_Is_Not_Returned_And_Valid(
            CacheReservationEmployerCommand command,
            [Frozen] Mock<IMediator> mockMediator,
            [Frozen]Mock<IFundingRulesService> rulesService,
            CacheReservationEmployerCommandValidator validator)
        {
            SetupAccountLegalEntityAsNonLevy(mockMediator);
            rulesService.Setup(x => x.GetFundingRules()).ReturnsAsync(new GetFundingRulesApiResponse
            {
                GlobalRules = new List<GlobalRule>
                {
                    new GlobalRule
                    {
                        Restriction = AccountRestriction.NonLevy,
                        RuleType = GlobalRuleType.FundingPaused,
                        ActiveFrom = DateTime.UtcNow.AddMonths(1)
                    }
                }
            });

            var result = await validator.ValidateAsync(command);

            result.IsValid().Should().BeTrue();
            result.FailedGlobalRuleValidation.Should().BeFalse();
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
            [Frozen] Mock<IMediator> mockMediator,
            [Frozen]Mock<IFundingRulesService> rulesService,
            CacheReservationEmployerCommandValidator validator)
        {
            SetupAccountLegalEntityAsNonLevy(mockMediator);
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
            [Frozen] Mock<IMediator> mockMediator,
            [Frozen]Mock<IFundingRulesService> rulesService,
            CacheReservationEmployerCommandValidator validator)
        {
            SetupAccountLegalEntityAsNonLevy(mockMediator);
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
            [Frozen] Mock<IMediator> mockMediator,
            [Frozen]Mock<IFundingRulesService> rulesService,
            CacheReservationEmployerCommandValidator validator)
        {
            SetupAccountLegalEntityAsNonLevy(mockMediator);
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
            [Frozen] Mock<IMediator> mockMediator,
            [Frozen]Mock<IFundingRulesService> rulesService,
            CacheReservationEmployerCommandValidator validator)
        {
            SetupAccountLegalEntityAsNonLevy(mockMediator);
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
            [Frozen] Mock<IMediator> mockMediator,
            [Frozen]Mock<IFundingRulesService> rulesService,
            CacheReservationEmployerCommandValidator validator)
        {
            SetupAccountLegalEntityAsNonLevy(mockMediator);
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
        public async Task And_Provider_Is_Supplied_For_Empty_Cohort_Then_It_Is_Not_Validated_For_Account_Legal_Entity(
            CacheReservationEmployerCommand command,
            [Frozen] Mock<IMediator> mediator,
            [Frozen]GetTrustedEmployersResponse response,
            CacheReservationEmployerCommandValidator validator)
        {
            SetupAccountLegalEntityAsNonLevy(mediator);
            command.IsEmptyCohortFromSelect = true;
            mediator.Setup(m => m.Send(It.IsAny<GetTrustedEmployersQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            var result = await validator.ValidateAsync(command);

            result.IsValid().Should().BeTrue();
            result.FailedAuthorisationValidation.Should().BeFalse();
            mediator.Verify(x => x.Send(It.IsAny<GetTrustedEmployersQuery>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test, MoqAutoData]
        public async Task And_Provider_Is_Not_Trusted_For_Account_Legal_Entity_Then_Mark_As_Not_Authorised(
            CacheReservationEmployerCommand command,
            [Frozen] Mock<IMediator> mediator,
            [Frozen]GetTrustedEmployersResponse response,
            CacheReservationEmployerCommandValidator validator)
        {
            SetupAccountLegalEntityAsNonLevy(mediator);
            command.IsEmptyCohortFromSelect = false;
            mediator.Setup(m => m.Send(It.IsAny<GetTrustedEmployersQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            var result = await validator.ValidateAsync(command);

            result.IsValid().Should().BeTrue();
            result.FailedAuthorisationValidation.Should().BeTrue();
        }

        [Test, MoqAutoData]
        public async Task And_Provider_Is_Trusted_For_Account_Legal_Entity_Then_Mark_As_Authorised(
            CacheReservationEmployerCommand command,
            [Frozen] Mock<IMediator> mediator,
            [Frozen]GetTrustedEmployersResponse response,
            CacheReservationEmployerCommandValidator validator)
        {
            SetupAccountLegalEntityAsNonLevy(mediator);
            command.IsEmptyCohortFromSelect = false;
            response.Employers = new[]
            {
                new AccountLegalEntity
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
            [Frozen] Mock<IMediator> mockMediator,
            [Frozen]Mock<IFundingRulesService> rulesService,
            CacheReservationEmployerCommandValidator validator)
        {
            ConfigureRulesServiceWithNoGlobalRules(rulesService);
            SetupAccountLegalEntityAsNonLevy(mockMediator);

            var result = await validator.ValidateAsync(command);

            result.IsValid().Should().BeTrue();
            result.FailedRuleValidation.Should().BeFalse();
        }

        [Test, MoqAutoData]
        public async Task And_NonLevy_And_Is_Valid(
            CacheReservationEmployerCommand command,
            [Frozen]Mock<IFundingRulesService> rulesService,
            [Frozen] Mock<IMediator> mockMediator,
            CacheReservationEmployerCommandValidator validator)
        {
            ConfigureRulesServiceWithNoGlobalRules(rulesService);
            SetupAccountLegalEntityAsNonLevy(mockMediator);

            var result = await validator.ValidateAsync(command);

            result.IsValid().Should().BeTrue();
        }

        private void SetupAccountLegalEntityAsNonLevy(Mock<IMediator> mockMediator)
        {
            var fixture = new Fixture();
            fixture.Customize<AccountLegalEntity>(composer => composer
                    .With(entity => entity.IsLevy, false));
            var getLegalEntitiesResponse = fixture.Create<GetLegalEntitiesResponse>();
            
            mockMediator
                .Setup(mediator => mediator.Send(It.IsAny<GetLegalEntitiesQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(getLegalEntitiesResponse);
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