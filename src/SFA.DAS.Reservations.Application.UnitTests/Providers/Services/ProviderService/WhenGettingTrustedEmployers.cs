using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Domain.Employers;
using SFA.DAS.Reservations.Domain.Employers.Api;
using SFA.DAS.Reservations.Infrastructure.Api;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Reservations.Application.UnitTests.Providers.Services.ProviderService
{
    public class WhenGettingTrustedEmployers
    {
        [Test, MoqAutoData]
        public async Task Then_The_Api_Is_Called_To_Get_Trusted_Employers_By_Provider(
            uint ukPrn,
            string expectedBaseUrl,
            List<AccountLegalEntity> accountLegalEntities,
            [Frozen] Mock<IApiClient> apiClient,
            Application.Providers.Services.ProviderService providerService)
        {
            //Arrange
            
            apiClient.Setup(x =>
                    x.GetAll<AccountLegalEntity>(
                        It.Is<GetTrustedEmployersRequest>(c => c.GetAllUrl.Contains($"api/accountlegalentities/provider/{ukPrn}"))))
                .ReturnsAsync(accountLegalEntities);
            
            //Act
            var actual = await providerService.GetTrustedEmployers(ukPrn);
            
            //Assert
            apiClient.Verify(x=>x.GetAll<AccountLegalEntity>(
                It.Is<GetTrustedEmployersRequest>(c=>c.GetAllUrl.Contains($"api/accountlegalentities/provider/{ukPrn}"))), Times.Once);
            actual.Should().BeEquivalentTo(accountLegalEntities);
        }
    }
}