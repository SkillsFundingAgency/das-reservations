using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using AutoFixture;
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
    public class WhenGettingAvailableDates
    {
        private IFundingRulesService _service;
        private Mock<IApiClient> _apiClient;
        private Mock<IOptions<ReservationsApiConfiguration>> _options;
        private const string ExpectedBaseUrl = "https://test.local/";
        private List<TraningDateModel> _expectedAvailableDates;
        private long _accountLegalEntityId;

        [SetUp]
        public void Arrange()
        {
            var fixture = new Fixture();

            _accountLegalEntityId = fixture.Create<long>();
            _expectedAvailableDates = fixture.Create<List<TraningDateModel>>();
            
            _apiClient = new Mock<IApiClient>();
            _apiClient.Setup(x =>
                    x.Get<GetAvailableDatesApiResponse>(
                        It.Is<GetAvailableDatesApiRequest>(c =>
                            c.GetUrl.Equals(
                                $"{ExpectedBaseUrl}api/rules/available-dates/{_accountLegalEntityId}"))))
                .ReturnsAsync(new GetAvailableDatesApiResponse
                {
                    AvailableDates = _expectedAvailableDates
                });

            _options = new Mock<IOptions<ReservationsApiConfiguration>>();
            _options
                .Setup(x => x.Value.Url)
                .Returns(ExpectedBaseUrl);

            _service = new FundingRulesService(_apiClient.Object, _options.Object);
        }

        [Test]
        public async Task Then_The_Available_Dates_Are_Returned()
        {
            //Act
            var result = await _service.GetAvailableDates(_accountLegalEntityId);

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
            var actualException = Assert.ThrowsAsync<WebException>(() => _service.GetAvailableDates(_accountLegalEntityId));
            Assert.AreEqual(exception, actualException);
        }
    }
}
