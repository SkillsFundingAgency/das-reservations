using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.NUnit3;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.Reservations.Commands.CacheReservationEmployer;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Domain.Rules;
using SFA.DAS.Reservations.Domain.Rules.Api;

namespace SFA.DAS.Reservations.Application.UnitTests.Reservations.Commands.CacheReservationEmployer
{
    [TestFixture]
    public class WhenValidatingACacheReservationEmployerCommand
    {
        private CacheReservationEmployerCommandValidator _validator;
        private Mock<IFundingRulesService> _rulesService;

        [SetUp]
        public void Arrange()
        {
            var fixture = new Fixture()
                .Customize(new AutoMoqCustomization { ConfigureMembers = true });

            _rulesService = fixture.Freeze<Mock<IFundingRulesService>>();
            _rulesService.Setup(x => x.GetAccountFundingRules(It.IsAny<long>())).ReturnsAsync(
                new GetAccountFundingRulesApiResponse
                {
                    GlobalRules = new List<GlobalRule>()
                });

            _validator = fixture.Create<CacheReservationEmployerCommandValidator>();
        }
        
        [Test, AutoData]
        public async Task And_No_Id_Then_Invalid(
            CacheReservationEmployerCommand command)
        {
            command.Id = Guid.Empty;

            var result = await _validator.ValidateAsync(command);

            result.IsValid().Should().BeFalse();
            result.ValidationDictionary.Count.Should().Be(1);
            result.ValidationDictionary
                .Should().ContainKey(nameof(CacheReservationEmployerCommand.Id))
                .WhichValue.Should().Be($"{nameof(CacheReservationEmployerCommand.Id)} has not been supplied");
        }

        [Test, AutoData]
        public async Task And_AccountId_Less_Than_One_Then_Invalid(
            CacheReservationEmployerCommand command)
        {
            command.AccountId = default(long);

            var result = await _validator.ValidateAsync(command);

            result.IsValid().Should().BeFalse();
            result.ValidationDictionary.Count.Should().Be(1);
            result.ValidationDictionary
                .Should().ContainKey(nameof(CacheReservationEmployerCommand.AccountId))
                .WhichValue.Should().Be($"{nameof(CacheReservationEmployerCommand.AccountId)} has not been supplied");
        }

        [Test, AutoData]
        public async Task And_AccountLegalEntityId_Less_Than_One_Then_Invalid(
            CacheReservationEmployerCommand command)
        {
            command.AccountLegalEntityId = default(long);

            var result = await _validator.ValidateAsync(command);

            result.IsValid().Should().BeFalse();
            result.ValidationDictionary.Count.Should().Be(1);
            result.ValidationDictionary
                .Should().ContainKey(nameof(CacheReservationEmployerCommand.AccountLegalEntityId))
                .WhichValue.Should().Be($"{nameof(CacheReservationEmployerCommand.AccountLegalEntityId)} has not been supplied");
        }

        [Test, AutoData]
        public async Task And_AccountLegalEntityName_Null_Then_Invalid(
            CacheReservationEmployerCommand command)
        {
            command.AccountLegalEntityName = null;

            var result = await _validator.ValidateAsync(command);

            result.IsValid().Should().BeFalse();
            result.ValidationDictionary.Count.Should().Be(1);
            result.ValidationDictionary
                .Should().ContainKey(nameof(CacheReservationEmployerCommand.AccountLegalEntityName))
                .WhichValue.Should().Be($"{nameof(CacheReservationEmployerCommand.AccountLegalEntityName)} has not been supplied");
        }

        [Test, AutoData]
        public async Task And_AccountLegalEntityName_Whitespace_Then_Invalid(
            CacheReservationEmployerCommand command)
        {
            command.AccountLegalEntityName = " ";

            var result = await _validator.ValidateAsync(command);

            result.IsValid().Should().BeFalse();
            result.ValidationDictionary.Count.Should().Be(1);
            result.ValidationDictionary
                .Should().ContainKey(nameof(CacheReservationEmployerCommand.AccountLegalEntityName))
                .WhichValue.Should().Be($"{nameof(CacheReservationEmployerCommand.AccountLegalEntityName)} has not been supplied");
        }

        [Test, AutoData]
        public async Task And_AccountLegalEntityPublicHashedId_Null_Then_Invalid(
            CacheReservationEmployerCommand command)
        {
            command.AccountLegalEntityPublicHashedId = null;

            var result = await _validator.ValidateAsync(command);

            result.IsValid().Should().BeFalse();
            result.ValidationDictionary.Count.Should().Be(1);
            result.ValidationDictionary
                .Should().ContainKey(nameof(CacheReservationEmployerCommand.AccountLegalEntityPublicHashedId))
                .WhichValue.Should().Be($"{nameof(CacheReservationEmployerCommand.AccountLegalEntityPublicHashedId)} has not been supplied");
        }

        [Test, AutoData]
        public async Task And_AccountLegalEntityPublicHashedId_Whitespace_Then_Invalid(
            CacheReservationEmployerCommand command)
        {
            command.AccountLegalEntityPublicHashedId = " ";

            var result = await _validator.ValidateAsync(command);

            result.IsValid().Should().BeFalse();
            result.ValidationDictionary.Count.Should().Be(1);
            result.ValidationDictionary
                .Should().ContainKey(nameof(CacheReservationEmployerCommand.AccountLegalEntityPublicHashedId))
                .WhichValue.Should().Be($"{nameof(CacheReservationEmployerCommand.AccountLegalEntityPublicHashedId)} has not been supplied");
        }

        [Test, AutoData]
        public async Task And_All_Properties_Ok_Then_Valid(
            CacheReservationEmployerCommand command)
        {
            var result = await _validator.ValidateAsync(command);

            result.IsValid().Should().BeTrue();
        }

        [Test, AutoData]
        public async Task Then_The_Account_Is_Checked_Against_The_AccountRules_And_An_Error_Returned_If_Not_Valid(
            CacheReservationEmployerCommand command)
        {
            _rulesService.Setup(x => x.GetAccountFundingRules(command.AccountId)).ReturnsAsync(
                new GetAccountFundingRulesApiResponse
                {
                    GlobalRules = new List<GlobalRule> {new GlobalRule
                    {
                        Restriction = AccountRestriction.Account,
                        RuleType = GlobalRuleType.ReservationLimit
                    }}
                });

            var result = await _validator.ValidateAsync(command);

            result.IsValid().Should().BeFalse();
            result.ValidationDictionary
                .Should().ContainKey(nameof(CacheReservationEmployerCommand.AccountId))
                .WhichValue.Should().Be("Reservation limit has been reached for this account");
        }
    }
}