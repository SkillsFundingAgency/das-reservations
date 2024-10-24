using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Domain.Employers;
using SFA.DAS.Reservations.Domain.Employers.Api;
using SFA.DAS.Reservations.Infrastructure.Api;
using SFA.DAS.Reservations.Infrastructure.Configuration;

namespace SFA.DAS.Reservations.Application.UnitTests.Providers.Services.ProviderService
{
    public class WhenGettingAccountLegalEntity
    {
        private const string ExpectedBaseUrl = "https://test.local/";

        private Application.Providers.Services.ProviderService _service;
        private Mock<IApiClient> _apiClient;
        private AccountLegalEntity _expectedLegalEntity;
        private Mock<IOptions<ReservationsApiConfiguration>> _options;

        [SetUp]
        public void Arrange()
        {
            _expectedLegalEntity = new AccountLegalEntity();

            _apiClient = new Mock<IApiClient>();
            var config = new ReservationsApiConfiguration
            {
                Url = ExpectedBaseUrl
            };

            _options = new Mock<IOptions<ReservationsApiConfiguration>>();
            _options.Setup(opt => opt.Value).Returns(config);

            _service = new Application.Providers.Services.ProviderService(_apiClient.Object, _options.Object);

            _apiClient.Setup(c => c.Get<AccountLegalEntity>(It.IsAny<GetAccountLegalEntityRequest>()))
                .ReturnsAsync(_expectedLegalEntity);
        }

        [Test]
        public async Task ThenWillCallApiToGetAccountLegalEntity()
        {
            //Arrange
            const int entityId = 23;

            //Act
            await _service.GetAccountLegalEntityById(entityId);

            //Assert
            _apiClient.Verify(c => c.Get<AccountLegalEntity>(It.Is<GetAccountLegalEntityRequest>(r => 
                r.Id.Equals(entityId) &&
                r.BaseUrl.Equals(ExpectedBaseUrl
                ))));
        }

        [Test]
        public async Task ThenWillReturnRetrievedAccountLegalEntity()
        {
            //Act
            var actualEntity = await _service.GetAccountLegalEntityById(23);

            //Assert
            actualEntity.Should().Be(_expectedLegalEntity);
        }

        [Test]
        public void ThenWillThrowExceptionIfOneIsRaised()
        {
            //Arrange
            var expectedException = new WebException("Test Error");

            _apiClient.Setup(c => c.Get<AccountLegalEntity>(It.IsAny<GetAccountLegalEntityRequest>()))
                      .ThrowsAsync(expectedException);

            //Act
            var actualException = Assert.ThrowsAsync<WebException>(() => _service.GetAccountLegalEntityById(23));

            //Assert
            actualException.Should().BeEquivalentTo(expectedException);
        }
    }
}
