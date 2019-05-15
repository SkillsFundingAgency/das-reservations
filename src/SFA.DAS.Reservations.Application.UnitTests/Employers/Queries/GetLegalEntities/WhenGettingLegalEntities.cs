using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.Employers.Queries.GetLegalEntities;
using SFA.DAS.Reservations.Domain.Employers;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Domain.Reservations.Api;
using SFA.DAS.Reservations.Infrastructure.Api;
using SFA.DAS.Reservations.Infrastructure.Configuration;

namespace SFA.DAS.Reservations.Application.UnitTests.Employers.Queries.GetLegalEntities
{
    [TestFixture]
    public class WhenGettingLegalEntities
    {
        [Test, MoqAutoData]
        public async Task And_No_Cache_Then_Gets_Legal_Entities_For_Account(
            GetLegalEntitiesQuery query,
            IEnumerable<AccountLegalEntity> legalEntities,
            [Frozen] Mock<IApiClient> mockApiClient,
            [Frozen] Mock<ICacheStorageService> mockCacheService,
            GetLegalEntitiesQueryHandler handler)
        {
            mockCacheService
                .Setup(service => service.RetrieveFromCache<IEnumerable<AccountLegalEntity>>(query.AccountId))
                .ReturnsAsync((IEnumerable<AccountLegalEntity>)null);
            
            mockApiClient
                .Setup(client => client.GetAll<AccountLegalEntity>(It.IsAny<GetAccountLegalEntitiesRequest>()))
                .ReturnsAsync(legalEntities);

            await handler.Handle(query, CancellationToken.None);

            mockApiClient.Verify(client => client.GetAll<AccountLegalEntity>(It.Is<GetAccountLegalEntitiesRequest>(r => r.AccountId.ToString().Equals(query.AccountId))), Times.Once);
        }

        [Test, MoqAutoData]
        public async Task Then_Uses_correct_Api_Base_Url(
            GetLegalEntitiesQuery query,
            IEnumerable<AccountLegalEntity> legalEntities,
            [Frozen] Mock<IApiClient> mockApiClient,
            [Frozen] Mock<ICacheStorageService> mockCacheService,
            [Frozen] Mock<IOptions<ReservationsApiConfiguration>> mockOptions,
            [Frozen] ReservationsApiConfiguration configuration,
            GetLegalEntitiesQueryHandler handler)
        {
            mockOptions.Setup(c => c.Value).Returns(configuration);

            mockCacheService
                .Setup(service => service.RetrieveFromCache<IEnumerable<AccountLegalEntity>>(query.AccountId))
                .ReturnsAsync((IEnumerable<AccountLegalEntity>)null);

            mockApiClient
                .Setup(client => client.GetAll<AccountLegalEntity>(It.IsAny<GetAccountLegalEntitiesRequest>()))
                .ReturnsAsync(legalEntities);
            
            await handler.Handle(query, CancellationToken.None);

            mockApiClient.Verify(client => client.GetAll<AccountLegalEntity>(It.Is<GetAccountLegalEntitiesRequest>(r => r.BaseUrl.Equals(configuration.Url))), Times.Once);
        }

        [Test, MoqAutoData]
        public async Task And_Is_Cache_Then_Gets_Legal_Entities_From_Cache(
            GetLegalEntitiesQuery query,
            IEnumerable<AccountLegalEntity> cachedLegalEntities,
            [Frozen] Mock<IApiClient> mockApiClient,
            [Frozen] Mock<ICacheStorageService> mockCacheService,
            GetLegalEntitiesQueryHandler handler)
        {
            mockCacheService
                .Setup(service => service.RetrieveFromCache<IEnumerable<AccountLegalEntity>>(query.AccountId))
                .ReturnsAsync(cachedLegalEntities);

            await handler.Handle(query, CancellationToken.None);
            
            mockApiClient.Verify(client => client.GetAll<AccountLegalEntity>(It.IsAny<GetAccountLegalEntitiesRequest>()), Times.Never);
        }


        [Test, MoqAutoData]
        public async Task Then_Caches_Legal_Entities(
            GetLegalEntitiesQuery query,
            IEnumerable<AccountLegalEntity> legalEntities,
            [Frozen] Mock<IApiClient> mockApiClient,
            [Frozen] Mock<ICacheStorageService> mockCacheService,
            GetLegalEntitiesQueryHandler handler)
        {
            mockCacheService
                .Setup(service => service.RetrieveFromCache<IEnumerable<AccountLegalEntity>>(query.AccountId))
                .ReturnsAsync((IEnumerable<AccountLegalEntity>)null);

            mockApiClient
                .Setup(client => client.GetAll<AccountLegalEntity>(It.IsAny<GetAccountLegalEntitiesRequest>()))
                .ReturnsAsync(legalEntities);
            
            await handler.Handle(query, CancellationToken.None);

            mockCacheService.Verify(service => service.SaveToCache(query.AccountId, It.IsAny<IEnumerable<AccountLegalEntity>>(), 1), Times.Once);
        }

        [Test, MoqAutoData]
        public async Task Then_Does_Not_Caches_Null_Legal_Entities(
            GetLegalEntitiesQuery query,
            IEnumerable<AccountLegalEntity> legalEntities,
            [Frozen] Mock<IApiClient> mockApiClient,
            [Frozen] Mock<ICacheStorageService> mockCacheService,
            GetLegalEntitiesQueryHandler handler)
        {
            mockCacheService
                .Setup(service => service.RetrieveFromCache<IEnumerable<AccountLegalEntity>>(query.AccountId))
                .ReturnsAsync((IEnumerable<AccountLegalEntity>)null);

            mockApiClient
                .Setup(client => client.GetAll<AccountLegalEntity>(It.IsAny<GetAccountLegalEntitiesRequest>()))
                .ReturnsAsync((IEnumerable<AccountLegalEntity>) null);
            
            await handler.Handle(query, CancellationToken.None);

            mockCacheService.Verify(service => service.SaveToCache(It.IsAny<string>(), It.IsAny<IEnumerable<AccountLegalEntity>>(), It.IsAny<int>()), Times.Never);
        }

        [Test, MoqAutoData]
        public async Task Then_Does_Not_Caches_Empty_Set_Of_Legal_Entities(
            GetLegalEntitiesQuery query,
            IEnumerable<AccountLegalEntity> legalEntities,
            [Frozen] Mock<IApiClient> mockApiClient,
            [Frozen] Mock<ICacheStorageService> mockCacheService,
            GetLegalEntitiesQueryHandler handler)
        {
            mockCacheService
                .Setup(service => service.RetrieveFromCache<IEnumerable<AccountLegalEntity>>(query.AccountId))
                .ReturnsAsync((IEnumerable<AccountLegalEntity>)null);

            mockApiClient
                .Setup(client => client.GetAll<AccountLegalEntity>(It.IsAny<GetAccountLegalEntitiesRequest>()))
                .ReturnsAsync(new List<AccountLegalEntity>());
            
            await handler.Handle(query, CancellationToken.None);

            mockCacheService.Verify(service => service.SaveToCache(It.IsAny<string>(), It.IsAny<IEnumerable<AccountLegalEntity>>(), It.IsAny<int>()), Times.Never);
        }

        [Test, MoqAutoData]
        public async Task Then_Returns_Legal_Entities(
            GetLegalEntitiesQuery query,
            IEnumerable<AccountLegalEntity> legalEntities,
            [Frozen] Mock<IApiClient> mockApiClient,
            [Frozen] Mock<ICacheStorageService> mockCacheService,
            GetLegalEntitiesQueryHandler handler)
        {
            mockCacheService
                .Setup(service => service.RetrieveFromCache<IEnumerable<AccountLegalEntity>>(query.AccountId))
                .ReturnsAsync((IEnumerable<AccountLegalEntity>)null);

            mockApiClient
                .Setup(client => client.GetAll<AccountLegalEntity>(It.IsAny<GetAccountLegalEntitiesRequest>()))
                .ReturnsAsync(legalEntities);
            
            var result = await handler.Handle(query, CancellationToken.None);

            result.AccountLegalEntities.Count().Should().Be(legalEntities.Count());
        }
    }
}