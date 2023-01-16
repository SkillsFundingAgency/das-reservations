using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Domain.Reservations;
using SFA.DAS.Reservations.Domain.Reservations.Api;
using SFA.DAS.Reservations.Infrastructure.Api;
using SFA.DAS.Reservations.Infrastructure.Configuration;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Reservations.Infrastructure.UnitTests.Services.ReservationOuterService
{
    public class WhenGettingTransferValidity
    {
        [Test, MoqAutoData]
        public async Task Then_Gets_Transfer_Validity(
            long senderId,
            long receiverId,
            GetTransferValidityResponse getTransferValidityResponse,
            [Frozen] Mock<IReservationsOuterApiClient> reservationsOuterApiClient,
            [Frozen] Mock<IOptions<ReservationsOuterApiConfiguration>> outerApiConfiguration,
            Infrastructure.Services.ReservationsOuterService service)
        {
            outerApiConfiguration.Object.Value.ApiBaseUrl = "https://tempuri.org";
            var expectedRequest =
                new GetTransferValidityRequest(outerApiConfiguration.Object.Value.ApiBaseUrl, senderId, receiverId, null);

            reservationsOuterApiClient.Setup(x => x.Get<GetTransferValidityResponse>(
                    It.Is<GetTransferValidityRequest>(c => c.GetUrl.Equals(expectedRequest.GetUrl))))
                .ReturnsAsync(getTransferValidityResponse);

            var result = await service.GetTransferValidity(senderId, receiverId, null);

            result.Should().BeEquivalentTo(getTransferValidityResponse);
        }

        [Test, MoqAutoData]
        public async Task With_BaseUrl_Trailing_Slash_Then_Gets_Transfer_Validity(
            long senderId,
            long receiverId,
            GetTransferValidityResponse getTransferValidityResponse,
            [Frozen] Mock<IReservationsOuterApiClient> reservationsOuterApiClient,
            [Frozen] Mock<IOptions<ReservationsOuterApiConfiguration>> outerApiConfiguration,
            Infrastructure.Services.ReservationsOuterService service)
        {
            outerApiConfiguration.Object.Value.ApiBaseUrl = "https://tempuri.org/";
            var expectedRequest =
                new GetTransferValidityRequest(outerApiConfiguration.Object.Value.ApiBaseUrl, senderId, receiverId, null);

            reservationsOuterApiClient.Setup(x => x.Get<GetTransferValidityResponse>(
                    It.Is<GetTransferValidityRequest>(c => c.GetUrl.Equals(expectedRequest.GetUrl))))
                .ReturnsAsync(getTransferValidityResponse);

            var result = await service.GetTransferValidity(senderId, receiverId, null);

            result.Should().BeEquivalentTo(getTransferValidityResponse);
        }

        [Test, MoqAutoData]
        public async Task With_PledgeApplicationId_Then_Gets_Transfer_Validity(
            long senderId,
            long receiverId,
            int pledgeApplicationId,
            GetTransferValidityResponse getTransferValidityResponse,
            [Frozen] Mock<IReservationsOuterApiClient> reservationsOuterApiClient,
            [Frozen] Mock<IOptions<ReservationsOuterApiConfiguration>> outerApiConfiguration,
            Infrastructure.Services.ReservationsOuterService service)
        {
            outerApiConfiguration.Object.Value.ApiBaseUrl = "https://tempuri.org";
            var expectedRequest =
                new GetTransferValidityRequest(outerApiConfiguration.Object.Value.ApiBaseUrl, senderId, receiverId, pledgeApplicationId);

            reservationsOuterApiClient.Setup(x => x.Get<GetTransferValidityResponse>(
                    It.Is<GetTransferValidityRequest>(c => c.GetUrl.Equals(expectedRequest.GetUrl))))
                .ReturnsAsync(getTransferValidityResponse);

            var result = await service.GetTransferValidity(senderId, receiverId, pledgeApplicationId);

            result.Should().BeEquivalentTo(getTransferValidityResponse);
        }
    }
}
