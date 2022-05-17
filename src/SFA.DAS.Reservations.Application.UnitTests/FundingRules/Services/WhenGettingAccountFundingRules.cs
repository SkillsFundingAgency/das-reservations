using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
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
    public class WhenGettingAccountFundingRules
    {
        private IFundingRulesService _service;
        private Mock<IApiClient> _apiClient;
        private Mock<IOptions<ReservationsApiConfiguration>> _options;
        private const string ExpectedBaseUrl = "https://test.local/";
        private List<GlobalRule> _expectedGlobalRules;
        private const long ExpectedAccountId = 3534234;

        [SetUp]
        public void Arrange()
        {
            _expectedGlobalRules = new List<GlobalRule>
            {
                new GlobalRule()
            };

            _apiClient = new Mock<IApiClient>();
            _apiClient.Setup(x =>
                    x.Get<GetAccountFundingRulesApiResponse>(
                        It.Is<GetAccountFundingRulesApiRequest>(c =>
                            c.GetUrl.Equals(
                                $"{ExpectedBaseUrl}api/rules/account/{ExpectedAccountId}"))))
                .ReturnsAsync(new GetAccountFundingRulesApiResponse
                {
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
        public async Task Then_The_GlobalRules_Are_Returned_For_That_Account()
        {
            //Act
            var result = await _service.GetAccountFundingRules(ExpectedAccountId);

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(_expectedGlobalRules, result.GlobalRules);
        }

        [Test]
        public void Then_Throws_WebException_If_Api_Call_Fails()
        {
            //Arrange
            var exception = new WebException();
            _apiClient.Setup(x =>
                    x.Get<GetAccountFundingRulesApiResponse>(
                        It.IsAny<GetAccountFundingRulesApiRequest>()))
                .ThrowsAsync(exception);

            //Act + Assert
            var actualException = Assert.ThrowsAsync<WebException>(() => _service.GetAccountFundingRules(ExpectedAccountId));
            Assert.AreEqual(exception, actualException);
        }
    }
}
