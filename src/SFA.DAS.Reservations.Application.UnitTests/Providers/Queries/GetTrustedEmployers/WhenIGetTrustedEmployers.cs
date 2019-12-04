using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Encoding;
using SFA.DAS.Reservations.Application.Providers.Queries.GetTrustedEmployers;
using SFA.DAS.Reservations.Application.Providers.Services;
using SFA.DAS.Reservations.Application.Validation;
using SFA.DAS.Reservations.Domain.Employers;
using ValidationResult = SFA.DAS.Reservations.Application.Validation.ValidationResult;


namespace SFA.DAS.Reservations.Application.UnitTests.Providers.Queries.GetTrustedEmployers
{
    public class WhenIGetTrustedEmployers
    {
        public const uint ExpectedUkPrn = 12345;

        private GetTrustedEmployersQueryHandler _handler;
        private Mock<IProviderService> _providerService;
        private GetTrustedEmployersQuery _query;
        private IList<AccountLegalEntity> _expectedAccountLegalEntities;
        private Mock<IValidator<GetTrustedEmployersQuery>> _validator;
        private Mock<IEncodingService> _encodingService;

        [SetUp]
        public void Arrange()
        {
            _expectedAccountLegalEntities = new List<AccountLegalEntity>
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

            _encodingService = new Mock<IEncodingService>();
            _encodingService.Setup(x => x.Encode(It.IsAny<long>(), EncodingType.PublicAccountLegalEntityId))
                .Returns("ABC123");
            _encodingService.Setup(x => x.Encode(It.IsAny<long>(), EncodingType.PublicAccountId))
                .Returns("DEF456");
            
            _providerService = new Mock<IProviderService>();
            _validator = new Mock<IValidator<GetTrustedEmployersQuery>>();

            _handler = new GetTrustedEmployersQueryHandler(_providerService.Object, _encodingService.Object, _validator.Object);

            _providerService.Setup(s => s.GetTrustedEmployers(ExpectedUkPrn)).ReturnsAsync(_expectedAccountLegalEntities);
            _validator.Setup(v => v.ValidateAsync(It.IsAny<GetTrustedEmployersQuery>()))
                .ReturnsAsync(new ValidationResult());
        }

        [Test] 
        public async Task Then_Trusted_Employers_Will_Be_Returned()
        {
            //Act
            var result = await _handler.Handle(_query, CancellationToken.None);

            //Assert
            result.Employers.Should().BeEquivalentTo(_expectedAccountLegalEntities);
            result.Employers.All(c => c.AccountPublicHashedId.Equals("DEF456")).Should().BeTrue();
            result.Employers.All(c => c.AccountLegalEntityPublicHashedId.Equals("ABC123")).Should().BeTrue();
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
