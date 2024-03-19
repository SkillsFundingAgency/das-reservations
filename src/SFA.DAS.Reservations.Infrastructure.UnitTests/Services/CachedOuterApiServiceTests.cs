using System;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Infrastructure.Services;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Reservations.Infrastructure.UnitTests.Services;

public class CachedOuterApiServiceTests
{
    private Mock<ICacheStorageService> _mockCacheStorageService;
    private Mock<IReservationsOuterService> _mockOuterApiService;

    [SetUp]
    public void SetUp()
    {
        _mockCacheStorageService = new Mock<ICacheStorageService>();
        _mockOuterApiService = new Mock<IReservationsOuterService>();
    }
    
    [Test, MoqAutoData]
    public async Task CanAccessCohort_Result_Is_Retrieved_From_OuterApiService_And_Stored_To_Cache_When_Not_In_Cache(
        Party party,
        long partyId,
        long cohortId,
        bool result)
    {
        var cacheKey = $"{nameof(CachedOuterApiService.CanAccessCohort)}.{party}.{partyId}.{cohortId}";

        _mockCacheStorageService
            .Setup(x => x.SafeRetrieveFromCache<bool?>(cacheKey))
            .ReturnsAsync((bool?)null);

        _mockOuterApiService.Setup(x => x.CanAccessCohort(party, partyId, cohortId)).ReturnsAsync(result);

        var sut = new CachedOuterApiService(_mockCacheStorageService.Object, _mockOuterApiService.Object);
        var actual = await sut.CanAccessCohort(party, partyId, cohortId);

        actual.Should().Be(result);

        _mockCacheStorageService.Verify(x => x.SafeRetrieveFromCache<bool?>(cacheKey), Times.Once);
        _mockCacheStorageService.Verify(x => x.SaveToCache(cacheKey, result, TimeSpan.FromMinutes(CachedOuterApiService.CacheExpirationMinutes)), Times.Once);
    }
    
    [Test, MoqAutoData]
    public async Task CanAccessCohort_Result_Is_Retrieved_From_Cache_When_Cached(
        Party party,
        long partyId,
        long cohortId,
        bool result)
    {
        var cacheKey = $"{nameof(CachedOuterApiService.CanAccessCohort)}.{party}.{partyId}.{cohortId}";

        _mockCacheStorageService
            .Setup(x => x.SafeRetrieveFromCache<bool?>(cacheKey))
            .ReturnsAsync(result);

        var sut = new CachedOuterApiService(_mockCacheStorageService.Object, _mockOuterApiService.Object);
        var actual = await sut.CanAccessCohort(party, partyId, cohortId);

        actual.Should().Be(result);

        _mockCacheStorageService.Verify(x => x.SafeRetrieveFromCache<bool?>(cacheKey), Times.Once);
        _mockCacheStorageService.Verify(x => x.SaveToCache(cacheKey, result, TimeSpan.FromMinutes(CachedOuterApiService.CacheExpirationMinutes)), Times.Never);
    }
}