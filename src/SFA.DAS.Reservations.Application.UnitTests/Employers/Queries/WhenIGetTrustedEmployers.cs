using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.Employers.Queries;
using SFA.DAS.Reservations.Application.Employers.Services;
using SFA.DAS.Reservations.Application.Validation;
using SFA.DAS.Reservations.Domain.Employers;
using ValidationResult = SFA.DAS.Reservations.Application.Validation.ValidationResult;


namespace SFA.DAS.Reservations.Application.UnitTests.Employers.Queries
{
    public class WhenIGetTrustedEmployers
    {
        public const uint ExpectedUkPrn = 12345;

        private GetTrustedEmployersQueryHandler _handler;
        private Mock<IProviderPermissionsService> _providerPermissionsService;
        private GetTrustedEmployersQuery _query;
        private IList<Employer> _expectedEmployers;
        private Mock<IValidator<GetTrustedEmployersQuery>> _validator;

        [SetUp]
        public void Arrange()
        {
            _expectedEmployers = new List<Employer>
            {
                new Employer
                {
                    AccountId = 1,
                    AccountName = "account 1",
                    AccountLegalEntityId = 11,
                    AccountLegalEntityName = "Entity 1"
                },
                new Employer
                {
                    AccountId = 2,
                    AccountName = "account 2",
                    AccountLegalEntityId = 22,
                    AccountLegalEntityName = "Entity 2"
                }
            };
            
            _query = new GetTrustedEmployersQuery
            {
                UkPrn = ExpectedUkPrn
            };

            _providerPermissionsService = new Mock<IProviderPermissionsService>();
            _validator = new Mock<IValidator<GetTrustedEmployersQuery>>();

            _handler = new GetTrustedEmployersQueryHandler(_providerPermissionsService.Object, _validator.Object);

            _providerPermissionsService.Setup(s => s.GetTrustedEmployers(ExpectedUkPrn)).ReturnsAsync(_expectedEmployers);
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
                .ReturnsAsync(new ValidationResult{ValidationDictionary = new Dictionary<string, string>{{"Test", "Test error"}}});

            //Act & Assert
            Assert.ThrowsAsync<ValidationException>(() => _handler.Handle(_query, CancellationToken.None));
        }
    }
}
