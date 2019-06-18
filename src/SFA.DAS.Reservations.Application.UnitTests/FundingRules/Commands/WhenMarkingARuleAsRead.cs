using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.FundingRules.Commands.MarkRuleAsRead;
using SFA.DAS.Reservations.Application.Validation;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Testing.AutoFixture;
using ValidationResult = SFA.DAS.Reservations.Application.Validation.ValidationResult;

namespace SFA.DAS.Reservations.Application.UnitTests.FundingRules.Commands
{
    public class WhenMarkingARuleAsRead
    {
        private MarkRuleAsReadCommandHandler _handler;
        private Mock<IFundingRulesService> _service;
        private Mock<IValidator< MarkRuleAsReadCommand>> _validator;

        [SetUp]
        public void Arrange()
        {
            _validator = new Mock<IValidator<MarkRuleAsReadCommand>>();
            _service = new Mock<IFundingRulesService>();

            _handler = new MarkRuleAsReadCommandHandler(_service.Object, _validator.Object);

            _validator.Setup(v => v.ValidateAsync(It.IsAny<MarkRuleAsReadCommand>()))
                .ReturnsAsync(() => new ValidationResult());
        }

        [Test, MoqAutoData]
        public async Task ThenWillSendCommandToMarkRuleAsRead(MarkRuleAsReadCommand command)
        {
            //Act
            await _handler.Handle(command, CancellationToken.None);

            //Assert
            _service.Verify(s => s.MarkRuleAsRead(command.Id, command.RuleId, command.TypeOfRule), Times.Once);
        }

        [Test, MoqAutoData]
        public void ThenWillThrowExceptionIfValidationFails(MarkRuleAsReadCommand command)
        {
            //Arrange
            _validator.Setup(v => v.ValidateAsync(It.IsAny<MarkRuleAsReadCommand>()))
                .ReturnsAsync(() => new ValidationResult{ValidationDictionary = new Dictionary<string, string>{{"Test", "Error"}}});

            //Act
            Assert.ThrowsAsync<ValidationException>(() => _handler.Handle(command, CancellationToken.None));

            //Assert
            _service.Verify(s => s.MarkRuleAsRead(command.Id, command.RuleId, command.TypeOfRule), Times.Never);
        }
    }
}
