using System;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.FundingRules.Commands.MarkRuleAsRead;
using SFA.DAS.Reservations.Domain.Rules.Api;

namespace SFA.DAS.Reservations.Application.UnitTests.FundingRules.Commands
{
    public class WhenValidatingCommand
    {
        [Test]
        public async Task And_All_Fields_Invalid_Then_Returns_All_Errors()
        {
            //Arrange
            var validator = new MarkRuleAsReadCommandValidator();

            var command = new  MarkRuleAsReadCommand
            {
                Id = "ABC",
                RuleId = -12,
                TypeOfRule = RuleType.None
            };

            //Act
            var result = await validator.ValidateAsync(command);

            //Assert
            result.IsValid().Should().BeFalse();
            result.ValidationDictionary.Count.Should().Be(3);
            result.ValidationDictionary
                .Should().ContainKey(nameof( MarkRuleAsReadCommand.Id))
                .And.ContainKey(nameof( MarkRuleAsReadCommand.RuleId))
                .And.ContainKey(nameof( MarkRuleAsReadCommand.TypeOfRule));
        }

        [Test]
        public async Task And_All_Fields_Not_Set_Then_Returns_All_Errors()
        {
            //Arrange
            var validator = new MarkRuleAsReadCommandValidator();

            var command = new  MarkRuleAsReadCommand();

            //Act
            var result = await validator.ValidateAsync(command);

            //Assert
            result.IsValid().Should().BeFalse();
            result.ValidationDictionary.Count.Should().Be(3);
            result.ValidationDictionary
                .Should().ContainKey(nameof( MarkRuleAsReadCommand.Id))
                .And.ContainKey(nameof( MarkRuleAsReadCommand.RuleId))
                .And.ContainKey(nameof( MarkRuleAsReadCommand.TypeOfRule));
        }

        [Test]
        public async Task And_All_Fields_For_Provider_Then_Command_Is_Valid()
        {
            //Arrange
            var validator = new MarkRuleAsReadCommandValidator();

            var command = new  MarkRuleAsReadCommand
            {
                Id = "123",
                RuleId = 1,
                TypeOfRule = RuleType.GlobalRule
            };

            //Act
            var result = await validator.ValidateAsync(command);
            
            //Assert
            result.IsValid().Should().BeTrue();
            result.ValidationDictionary.Count.Should().Be(0);
        }

        [Test]
        public async Task And_All_Fields_For_Employer_Then_Command_Is_Valid()
        {
            //Arrange
            var validator = new MarkRuleAsReadCommandValidator();

            var command = new  MarkRuleAsReadCommand
            {
                Id = Guid.NewGuid().ToString(),
                RuleId = 1,
                TypeOfRule = RuleType.GlobalRule
            };

            //Act
            var result = await validator.ValidateAsync(command);
            
            //Assert
            result.IsValid().Should().BeTrue();
            result.ValidationDictionary.Count.Should().Be(0);
        }
    }
}
