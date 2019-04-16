using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.FundingRules.Services;
using SFA.DAS.Reservations.Domain.Rules;
using SFA.DAS.Reservations.Domain.Rules.Api;
using SFA.DAS.Reservations.Infrastructure.Api;
using SFA.DAS.Reservations.Infrastructure.Configuration.Configuration;

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
                    CourseRules = _expectedResevationRules,
                    GlobalRules = _expectedGlobalRules
                });

            _options = new Mock<IOptions<ReservationsApiConfiguration>>();
            _options.Setup(x => x.Value.Url).Returns(ExpectedBaseUrl);

            _service = new FundingFundingRulesService(_apiClient.Object, _options.Object);
        }

        [Test]
        public async Task ThenRulesShouldBeReturned()
        {
            //Act
            var result = await _service.GetFundingRules();

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(_expectedResevationRules, result.CourseRules);
            Assert.AreEqual(_expectedGlobalRules, result.GlobalRules);
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
            Assert.AreEqual(exception, actualException);
        }
    }
}
