using AutoFixture.NUnit3;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Domain.Providers.Api;
using SFA.DAS.Reservations.Infrastructure.Api;
using SFA.DAS.Reservations.Infrastructure.Configuration;
using SFA.DAS.Testing.AutoFixture;
using System.Threading.Tasks;

namespace SFA.DAS.Reservations.Infrastructure.UnitTests.Services.ReservationOuterService
{
    public class WhenGettingProviderStatusTest
    {
        [Test, MoqAutoData]
        public async Task Then_The_Request_Is_Made_And_ProviderResponse_Returned(
            long ukprn,
            ProviderAccountResponse apiResponse,
            [Frozen] Mock<IReservationsOuterApiClient> reservationsOuterApiClient,
            [Frozen] Mock<IOptions<ReservationsOuterApiConfiguration>> outerApiConfiguration,
            Infrastructure.Services.ReservationsOuterService service)
        {
            //Arrange
            outerApiConfiguration.Object.Value.ApiBaseUrl = "https://tempuri.org";
            var expectedRequest = new GetProviderStatusDetails(outerApiConfiguration.Object.Value.ApiBaseUrl, ukprn);
            
            reservationsOuterApiClient.Setup(x => x.Get<ProviderAccountResponse>(
                    It.Is<GetProviderStatusDetails>(c => c.GetUrl.Equals(expectedRequest.GetUrl))))
                .ReturnsAsync(apiResponse);

            //Act
            var actual = await service.GetProviderStatus(ukprn);

            //Assert
            actual.CanAccessService.Should().Be(apiResponse.CanAccessService);
        }
    }
}
