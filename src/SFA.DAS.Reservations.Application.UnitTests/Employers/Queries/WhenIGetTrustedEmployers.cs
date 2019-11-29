using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.Employers.Queries;
using SFA.DAS.Reservations.Application.Providers.Queries;
using SFA.DAS.Reservations.Application.Providers.Queries.GetTrustedEmployers;
using SFA.DAS.Reservations.Application.Providers.Services;
using SFA.DAS.Reservations.Application.Validation;
using SFA.DAS.Reservations.Domain.Employers;
using SFA.DAS.Reservations.Domain.Interfaces;
using ValidationResult = SFA.DAS.Reservations.Application.Validation.ValidationResult;


namespace SFA.DAS.Reservations.Application.UnitTests.Employers.Queries
{
    public class WhenIGetTrustedEmployers
    {
        public const uint ExpectedUkPrn = 12345;

        private GetTrustedEmployersQueryHandler _handler;
        private Mock<IProviderService> _providerService;
        private GetTrustedEmployersQuery _query;
        private IList<AccountLegalEntity> _expectedEmployers;
        private Mock<IValidator<GetTrustedEmployersQuery>> _validator;

        [SetUp]
        public void Arrange()
        {
            _expectedEmployers = new List<AccountLegalEntity>
            {
                new AccountLegalEntity
                {
                    AccountId = 1,
                    AgreementSigned = false,
                    AccountName = "account 1",
                    AccountLegalEntityId = 11,
                    AccountLegalEntityName = "Entity 1"
                },
                new AccountLegalEntity
                {
                    AccountId = 2,
                    AgreementSigned = true,
                    AccountName = "account 2",
                    AccountLegalEntityId = 22,
                    AccountLegalEntityName = "Entity 2"
                }
            };
            
            _query = new GetTrustedEmployersQuery
            {
                UkPrn = ExpectedUkPrn
            };

            _providerService = new Mock<IProviderService>();
            _validator = new Mock<IValidator<GetTrustedEmployersQuery>>();

            _handler = new GetTrustedEmployersQueryHandler(_providerService.Object, _validator.Object);

            _providerService.Setup(s => s.GetTrustedEmployers(ExpectedUkPrn)).ReturnsAsync(_expectedEmployers);
            _validator.Setup(v => v.ValidateAsync(It.IsAny<GetTrustedEmployersQuery>()))
                .ReturnsAsync(new ValidationResult());
        }

        [Test] 
        public async Task Then_Trusted_Employers_Will_Be_Returned()
        {
            //Act
            var result = await _handler.Handle(_query, CancellationToken.None);

            //Assert
            result.Employers.Should().BeEquivalentTo(_expectedEmployers);
        }

        [Test]
        public void Then_Throws_Validation_Exception_If_Request_Is_Invalid()
        {
            //Assign
            _validator.Setup(v => v.ValidateAsync(It.IsAny<GetTrustedEmployersQuery>()))
                .ReturnsAsync(new ValidationResult{ValidationDictionary = new Dictionary<string, string>{{"Test", "Test error"}, {"Test2", "Test error2"}}});

            //Act & Assert
            Assert.ThrowsAsync<ValidationException>(() => _handler.Handle(_query, CancellationToken.None));
        }
    }
}
