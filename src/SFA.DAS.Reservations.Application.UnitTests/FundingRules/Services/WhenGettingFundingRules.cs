using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.FundingRules.Services;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Domain.Rules;
using SFA.DAS.Reservations.Domain.Rules.Api;
using SFA.DAS.Reservations.Infrastructure.Api;
using SFA.DAS.Reservations.Infrastructure.Configuration;

namespace SFA.DAS.Reservations.Application.UnitTests.FundingRules.Services
{
    public class WhenGettingFundingRules
    {
        private IFundingRulesService _service;
        private Mock<IApiClient> _apiClient;
        private Mock<IOptions<ReservationsApiConfiguration>> _options;
        private const string ExpectedBaseUrl = "https://test.local/";
        private List<ReservationRule> _expectedResevationRules;
        private List<GlobalRule> _expectedGlobalRules;

        [SetUp]
        public void Arrange()
        {
            _expectedResevationRules = new List<ReservationRule>
            {
                new ReservationRule()
            };
            
            _expectedGlobalRules = new List<GlobalRule>
            {
                new GlobalRule()
            };

            _apiClient = new Mock<IApiClient>();
            _apiClient.Setup(x =>
                    x.Get<GetFundingRulesApiResponse>(
                        It.Is<GetFundingRulesApiRequest>(c =>
                            c.GetUrl.Equals(
                                $"{ExpectedBaseUrl}api/rules"))))
                .ReturnsAsync(new GetFundingRulesApiResponse
                {
                    Rules = _expectedResevationRules,
                    GlobalRules = _expectedGlobalRules
                });

            var config = new ReservationsApiConfiguration
            {
                Url = ExpectedBaseUrl
            };

            _options = new Mock<IOptions<ReservationsApiConfiguration>>();
            _options.Setup(opt => opt.Value).Returns(config);

            _service = new FundingRulesService(_apiClient.Object, _options.Object);
        }

        [Test]
        public async Task ThenRulesShouldBeReturned()
        {
            //Act
            var result = await _service.GetFundingRules();

            //Assert
            result.Should().NotBeNull();
            result.Rules.Should().BeEquivalentTo(_expectedResevationRules);
            result.GlobalRules.Should().BeEquivalentTo(_expectedGlobalRules);
        }

        [Test]
        public void ThenThrowsExceptionIfApiCallFails()
        {
            //Arrange
            var exception = new WebException();
            _apiClient.Setup(x =>
                    x.Get<GetFundingRulesApiResponse>(
                        It.IsAny<GetFundingRulesApiRequest>()))
                .ThrowsAsync(exception);

            //Act + Assert
            var actualException = Assert.ThrowsAsync<WebException>(() => _service.GetFundingRules());
            actualException.Should().Be(exception);
        }
    }
}
