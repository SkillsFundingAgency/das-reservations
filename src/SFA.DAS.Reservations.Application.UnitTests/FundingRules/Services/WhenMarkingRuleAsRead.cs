using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.FundingRules.Services;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Domain.Rules.Api;
using SFA.DAS.Reservations.Infrastructure.Api;
using SFA.DAS.Reservations.Infrastructure.Configuration;

namespace SFA.DAS.Reservations.Application.UnitTests.FundingRules.Services
{
    public class WhenMarkingRuleAsRead
    {
        private IFundingRulesService _service;
        private Mock<IApiClient> _apiClient;
        private Mock<IOptions<ReservationsApiConfiguration>> _options;
        private const string ExpectedBaseUrl = "https://test.local/";

        [SetUp]
        public void Arrange()
        {
            _apiClient = new Mock<IApiClient>();
            _apiClient.Setup(x =>
                x.Create<bool>(It.Is<MarkRuleAsReadApiRequest>(c =>
                        c.CreateUrl.Equals(
                            $"{ExpectedBaseUrl}api/rules"))));

            var config = new ReservationsApiConfiguration
            {
                Url = ExpectedBaseUrl
            };

            _options = new Mock<IOptions<ReservationsApiConfiguration>>();
            _options.Setup(opt => opt.Value).Returns(config);

            _service = new FundingRulesService(_apiClient.Object, _options.Object);
        }

        [Test]
        public async Task ThenWillSendRequestToApi()
        {
            //arrange
            var expectedId = "123";
            var expectedRuleId = 2567;
            var expectedRuleType = RuleType.GlobalRule;

            //act
            await _service.MarkRuleAsRead(expectedId, expectedRuleId, expectedRuleType);

            //assign
            _apiClient.Verify(c => c.Create<MarkRuleAsReadApiResponse>(It.Is<MarkRuleAsReadApiRequest>(
                r => r.Id.Equals(expectedId) &&
                     r.RuleId.Equals(expectedRuleId) &&
                     r.TypeOfRule.Equals(expectedRuleType))), Times.Once);
        }

        [Test]
        public void ThenWillThrowExceptionFromApi()
        {
            //arrange
            var expectedException = new Exception();
            _apiClient.Setup(c => c.Create<MarkRuleAsReadApiResponse>(It.IsAny<MarkRuleAsReadApiRequest>()))
                      .Throws(expectedException);

            //act
            var actualException = Assert.ThrowsAsync<Exception>(() => _service.MarkRuleAsRead("1", 2, RuleType.GlobalRule));

            //assign
            Assert.AreEqual(expectedException, actualException);
        }
    }
}
