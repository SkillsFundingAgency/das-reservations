using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.FundingRules.Services;
using SFA.DAS.Reservations.Domain.Rules.Api;
using SFA.DAS.Reservations.Infrastructure.Api;
using SFA.DAS.Reservations.Infrastructure.Configuration;

namespace SFA.DAS.Reservations.Application.UnitTests.FundingRules.Services
{
    public class WhenGettingAvailableDates
    {
        private IFundingRulesService _service;
        private Mock<IApiClient> _apiClient;
        private Mock<IOptions<ReservationsApiConfiguration>> _options;
        private const string ExpectedBaseUrl = "https://test.local/";
        private List<DateTime> _expectedAvailableDates;

        [SetUp]
        public void Arrange()
        {
            _expectedAvailableDates = new List<DateTime>
            {
                new DateTime(2019,02,01),
                new DateTime(2019,03,01),
                new DateTime(2019,04,01),
            };


            _apiClient = new Mock<IApiClient>();
            _apiClient.Setup(x =>
                    x.Get<GetAvailableDatesApiResponse>(
                        It.Is<GetAvailableDatesApiRequest>(c =>
                            c.GetUrl.Equals(
                                $"{ExpectedBaseUrl}api/available-dates"))))
                .ReturnsAsync(new GetAvailableDatesApiResponse
                {
                    AvailableDates = _expectedAvailableDates
                });

            _options = new Mock<IOptions<ReservationsApiConfiguration>>();
            _options.Setup(x => x.Value.Url).Returns(ExpectedBaseUrl);

            _service = new FundingRulesService(_apiClient.Object, _options.Object);
        }

        [Test]
        public async Task ThenRulesShouldBeReturned()
        {
            //Act
            var result = await _service.GetAvailableDates();

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(_expectedAvailableDates, result.AvailableDates);
        }

        [Test]
        public void ThenThrowsExceptionIfApiCallFails()
        {
            //Arrange
            var exception = new WebException();
            _apiClient.Setup(x =>
                    x.Get<GetAvailableDatesApiResponse>(
                        It.IsAny<GetAvailableDatesApiRequest>()))
                .ThrowsAsync(exception);

            //Act + Assert
            var actualException = Assert.ThrowsAsync<WebException>(() => _service.GetAvailableDates());
            Assert.AreEqual(exception, actualException);
        }
    }
}
