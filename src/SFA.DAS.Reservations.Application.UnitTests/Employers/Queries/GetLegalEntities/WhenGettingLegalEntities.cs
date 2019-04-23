using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EAS.Account.Api.Client;
using SFA.DAS.EAS.Account.Api.Types;
using SFA.DAS.Reservations.Application.Employers.Queries.GetLegalEntities;
using SFA.DAS.Reservations.Domain.Employers;
using SFA.DAS.Reservations.Domain.Interfaces;

namespace SFA.DAS.Reservations.Application.UnitTests.Employers.Queries.GetLegalEntities
{
    [TestFixture]
    public class WhenGettingLegalEntities
    {
        [Test, MoqAutoData]
        public async Task And_No_Cache_Then_Gets_Legal_Entities_For_Account(
            GetLegalEntitiesQuery query,
            LegalEntityViewModel legalEntityViewModel,
            [Frozen] Mock<IAccountApiClient> mockAccountApiClient,
            [Frozen] Mock<ICacheStorageService> mockCacheService,
            GetLegalEntitiesQueryHandler handler)
        {
            mockCacheService
                .Setup(service => service.RetrieveFromCache<IEnumerable<AccountLegalEntity>>(query.AccountId))
                .ReturnsAsync((IEnumerable<AccountLegalEntity>)null);
            mockAccountApiClient
                .Setup(client => client.GetResource<LegalEntityViewModel>(It.IsAny<string>()))
                .ReturnsAsync(legalEntityViewModel);

            await handler.Handle(query, CancellationToken.None);

            mockAccountApiClient.Verify(client => client.GetLegalEntitiesConnectedToAccount(query.AccountId), Times.Once);
        }

        [Test, MoqAutoData]
        public async Task And_Is_Cache_Then_Gets_Legal_Entities_From_Cache(
            GetLegalEntitiesQuery query,
            IEnumerable<AccountLegalEntity> cachedLegalEntities,
            [Frozen] Mock<IAccountApiClient> mockAccountApiClient,
            [Frozen] Mock<ICacheStorageService> mockCacheService,
            GetLegalEntitiesQueryHandler handler)
        {
            mockCacheService
                .Setup(service => service.RetrieveFromCache<IEnumerable<AccountLegalEntity>>(query.AccountId))
                .ReturnsAsync(cachedLegalEntities);

            await handler.Handle(query, CancellationToken.None);

            mockAccountApiClient.Verify(client => client.GetLegalEntitiesConnectedToAccount(query.AccountId), Times.Never);
            mockAccountApiClient.Verify(client => client.GetResource<LegalEntityViewModel>(It.IsAny<string>()), Times.Never);
        }

        [Test, MoqAutoData]
        public async Task Then_Gets_Legal_Entities_Details(
            GetLegalEntitiesQuery query,
            List<ResourceViewModel> resourceViewModels,
            LegalEntityViewModel legalEntityViewModel,
            [Frozen] Mock<IAccountApiClient> mockAccountApiClient,
            [Frozen] Mock<ICacheStorageService> mockCacheService,
            GetLegalEntitiesQueryHandler handler)
        {
            mockCacheService
                .Setup(service => service.RetrieveFromCache<IEnumerable<AccountLegalEntity>>(query.AccountId))
                .ReturnsAsync((IEnumerable<AccountLegalEntity>)null);

            mockAccountApiClient
                .Setup(client => client.GetLegalEntitiesConnectedToAccount(It.IsAny<string>()))
                .ReturnsAsync(resourceViewModels);
            mockAccountApiClient
                .Setup(client => client.GetResource<LegalEntityViewModel>(It.IsAny<string>()))
                .ReturnsAsync(legalEntityViewModel);

            await handler.Handle(query, CancellationToken.None);

            resourceViewModels.ForEach(model => 
                mockAccountApiClient.Verify(client => client.GetResource<LegalEntityViewModel>(model.Href), Times.Once));
        }

        [Test, MoqAutoData]
        public async Task Then_Caches_Legal_Entities_Details(
            GetLegalEntitiesQuery query,
            List<ResourceViewModel> resourceViewModels,
            LegalEntityViewModel legalEntityViewModel,
            [Frozen] Mock<IAccountApiClient> mockAccountApiClient,
            [Frozen] Mock<ICacheStorageService> mockCacheService,
            GetLegalEntitiesQueryHandler handler)
        {
            mockCacheService
                .Setup(service => service.RetrieveFromCache<IEnumerable<AccountLegalEntity>>(query.AccountId))
                .ReturnsAsync((IEnumerable<AccountLegalEntity>)null);

            mockAccountApiClient
                .Setup(client => client.GetLegalEntitiesConnectedToAccount(It.IsAny<string>()))
                .ReturnsAsync(resourceViewModels);
            mockAccountApiClient
                .Setup(client => client.GetResource<LegalEntityViewModel>(It.IsAny<string>()))
                .ReturnsAsync(legalEntityViewModel);
            
            await handler.Handle(query, CancellationToken.None);

            mockCacheService.Verify(service => service.SaveToCache(query.AccountId, It.IsAny<IEnumerable<AccountLegalEntity>>(), 1), Times.Once);
        }

        [Test, MoqAutoData]
        public async Task Then_Returns_Legal_Entities(
            GetLegalEntitiesQuery query,
            List<ResourceViewModel> resourceViewModels,
            LegalEntityViewModel legalEntityViewModel,
            [Frozen] Mock<IAccountApiClient> mockAccountApiClient,
            [Frozen] Mock<ICacheStorageService> mockCacheService,
            GetLegalEntitiesQueryHandler handler)
        {
            mockCacheService
                .Setup(service => service.RetrieveFromCache<IEnumerable<AccountLegalEntity>>(query.AccountId))
                .ReturnsAsync((IEnumerable<AccountLegalEntity>)null);

            mockAccountApiClient
                .Setup(client => client.GetLegalEntitiesConnectedToAccount(It.IsAny<string>()))
                .ReturnsAsync(resourceViewModels);
            mockAccountApiClient
                .Setup(client => client.GetResource<LegalEntityViewModel>(It.IsAny<string>()))
                .ReturnsAsync(legalEntityViewModel);
            
            var result = await handler.Handle(query, CancellationToken.None);

            result.AccountLegalEntities.Count().Should().Be(resourceViewModels.Count);
            result.AccountLegalEntities.First().Should().BeEquivalentTo(legalEntityViewModel, options => options.ExcludingMissingMembers());
        }
    }
}