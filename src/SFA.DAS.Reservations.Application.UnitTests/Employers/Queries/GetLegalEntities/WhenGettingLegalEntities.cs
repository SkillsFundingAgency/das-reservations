using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using SFA.DAS.Encoding;
using SFA.DAS.Reservations.Application.Employers.Queries.GetLegalEntities;
using SFA.DAS.Reservations.Domain.Employers;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Domain.Reservations.Api;
using SFA.DAS.Testing.AutoFixture;
using SFA.DAS.Reservations.Infrastructure.Api;
using SFA.DAS.Reservations.Infrastructure.Configuration;

namespace SFA.DAS.Reservations.Application.UnitTests.Employers.Queries.GetLegalEntities
{
    [TestFixture]
    public class WhenGettingLegalEntities
    {
        [Test, MoqAutoData]
        public async Task Then_Gets_Legal_Entities_For_Account(
            GetLegalEntitiesQuery query,
            IEnumerable<AccountLegalEntity> legalEntities,
            [Frozen] Mock<IApiClient> mockApiClient,
            GetLegalEntitiesQueryHandler handler)
        {
            
            mockApiClient
                .Setup(client => client.GetAll<AccountLegalEntity>(It.IsAny<GetAccountLegalEntitiesRequest>()))
                .ReturnsAsync(legalEntities);

            await handler.Handle(query, CancellationToken.None);

            mockApiClient.Verify(client => client.GetAll<AccountLegalEntity>(It.Is<GetAccountLegalEntitiesRequest>(r => r.AccountId.Equals(query.AccountId))), Times.Once);
        }

        [Test, MoqAutoData]
        public async Task Then_Uses_correct_Api_Base_Url(
            GetLegalEntitiesQuery query,
            IEnumerable<AccountLegalEntity> legalEntities,
            [Frozen] Mock<IApiClient> mockApiClient,
            [Frozen] Mock<IOptions<ReservationsApiConfiguration>> mockOptions,
            [Frozen] Mock<IEncodingService> encodingService,
            [Frozen] ReservationsApiConfiguration configuration,
            GetLegalEntitiesQueryHandler handler)
        {
            mockOptions.Setup(c => c.Value).Returns(configuration);

            mockApiClient
                .Setup(client => client.GetAll<AccountLegalEntity>(It.IsAny<GetAccountLegalEntitiesRequest>()))
                .ReturnsAsync(legalEntities);
            
            await handler.Handle(query, CancellationToken.None);

            mockApiClient.Verify(client => client.GetAll<AccountLegalEntity>(It.Is<GetAccountLegalEntitiesRequest>(r => r.BaseUrl.Equals(configuration.Url))), Times.Once);
        }

        [Test, MoqAutoData]
        public async Task Then_Hashes_The_Id(
            GetLegalEntitiesQuery query,
            IEnumerable<AccountLegalEntity> legalEntities,
            [Frozen] Mock<IApiClient> mockApiClient,
            [Frozen] Mock<IEncodingService> encodingService,
            GetLegalEntitiesQueryHandler handler)
        {

            mockApiClient
                .Setup(client => client.GetAll<AccountLegalEntity>(It.IsAny<GetAccountLegalEntitiesRequest>()))
                .ReturnsAsync(legalEntities);
            
            await handler.Handle(query, CancellationToken.None);

            encodingService.Verify(service => service.Encode(legalEntities.FirstOrDefault().AccountLegalEntityId,EncodingType.PublicAccountLegalEntityId), Times.Once);
        }


        [Test, MoqAutoData]
        public async Task Then_Returns_Legal_Entities(
            GetLegalEntitiesQuery query,
            IEnumerable<AccountLegalEntity> legalEntities,
            [Frozen] Mock<IApiClient> mockApiClient,
            GetLegalEntitiesQueryHandler handler)
        {
            
            mockApiClient
                .Setup(client => client.GetAll<AccountLegalEntity>(It.IsAny<GetAccountLegalEntitiesRequest>()))
                .ReturnsAsync(legalEntities);
            
            var result = await handler.Handle(query, CancellationToken.None);

            result.AccountLegalEntities.Count().Should().Be(legalEntities.Count());
        }
    }
}