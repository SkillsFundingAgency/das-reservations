using System;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
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
        long providerId,
        long cohortId,
        bool result)
    {
        var cacheKey = $"{nameof(CachedReservationsOuterService.CanAccessCohort)}.{providerId}.{cohortId}";

        _mockCacheStorageService
            .Setup(x => x.SafeRetrieveFromCache<bool?>(cacheKey))
            .ReturnsAsync((bool?)null);

        _mockOuterApiService.Setup(x => x.CanAccessCohort(providerId, cohortId)).ReturnsAsync(result);

        var sut = new CachedReservationsOuterService(_mockCacheStorageService.Object, _mockOuterApiService.Object);
        var actual = await sut.CanAccessCohort(providerId, cohortId);

        actual.Should().Be(result);

        _mockCacheStorageService.Verify(x => x.SafeRetrieveFromCache<bool?>(cacheKey), Times.Once);
        _mockCacheStorageService.Verify(x => x.SaveToCache(cacheKey, result, TimeSpan.FromMinutes(CachedReservationsOuterService.CacheExpirationMinutes)), Times.Once);
    }
    
    [Test, MoqAutoData]
    public async Task CanAccessCohort_Result_Is_Retrieved_From_Cache_When_Cached(
        long providerId,
        long cohortId,
        bool result)
    {
        var cacheKey = $"{nameof(CachedReservationsOuterService.CanAccessCohort)}.{providerId}.{cohortId}";

        _mockCacheStorageService
            .Setup(x => x.SafeRetrieveFromCache<bool?>(cacheKey))
            .ReturnsAsync(result);

        var sut = new CachedReservationsOuterService(_mockCacheStorageService.Object, _mockOuterApiService.Object);
        var actual = await sut.CanAccessCohort(providerId, cohortId);

        actual.Should().Be(result);

        _mockCacheStorageService.Verify(x => x.SafeRetrieveFromCache<bool?>(cacheKey), Times.Once);
        _mockCacheStorageService.Verify(x => x.SaveToCache(cacheKey, result, TimeSpan.FromMinutes(CachedReservationsOuterService.CacheExpirationMinutes)), Times.Never);
    }
}