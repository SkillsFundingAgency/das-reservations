using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Domain.Employers;
using SFA.DAS.Reservations.Domain.Employers.Api;
using SFA.DAS.Reservations.Infrastructure.Api;
using SFA.DAS.Reservations.Infrastructure.Configuration;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Reservations.Application.UnitTests.Providers.Services.ProviderService
{
    public class WhenGettingTrustedEmployers
    {
        [Test, MoqAutoData]
        public async Task Then_The_Api_Is_Called_To_Get_Trusted_Employers(
            uint ukPrn,
            string expectedBaseUrl,
            [Frozen] Mock<IApiClient> apiClient,
            [Frozen] Mock<IOptions<ReservationsApiConfiguration>> options,
            Application.Providers.Services.ProviderService providerService)
        {
            //Arrange
            options.Setup(x => x.Value.Url).Returns(expectedBaseUrl);
            apiClient.Setup(x =>
                    x.GetAll<AccountLegalEntity>(
                        It.Is<GetTrustedEmployersRequest>(c => c.GetAllUrl.Equals(""))))
                .ReturnsAsync(new List<AccountLegalEntity>());
            
            //Act
            await providerService.GetTrustedEmployers(ukPrn);
        }
    }
}