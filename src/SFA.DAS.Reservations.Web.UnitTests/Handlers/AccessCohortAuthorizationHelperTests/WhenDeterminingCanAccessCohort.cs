using System;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Encoding;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Domain.Providers.Api;
using SFA.DAS.Reservations.Infrastructure.Services;
using SFA.DAS.Reservations.Web.Infrastructure;
using SFA.DAS.Reservations.Web.Infrastructure.Authorization;
using SFA.DAS.Reservations.Web.Infrastructure.Authorization.Helpers;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Reservations.Web.UnitTests.Handlers.AccessCohortAuthorizationHelperTests;

public class WhenDeterminingCanAccessCohort
{
    [Test, MoqAutoData]
    public async Task ThenReturnsFalseWhenUserClaimsAreEmpty(
        int ukprn,
        AccessCohortRequirement requirement,
        [Frozen] Mock<IHttpContextAccessor> httpContextAccessor,
        long accountLegalEntityHashedId,
        [Frozen] Mock<IReservationsOuterService> outerService,
        AccessCohortAuthorizationHelper sut)
    {
        var claimsPrinciple = new ClaimsPrincipal();

        var httpContext = new DefaultHttpContext(new FeatureCollection()) { User = claimsPrinciple };
        httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);
        httpContext.Request.RouteValues.Add(RouteValueKeys.AccountLegalEntityPublicHashedId, accountLegalEntityHashedId);
        httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        var actual = await sut.CanAccessCohort();

        actual.Should().BeFalse();
    }

    [Test, MoqAutoData]
    public async Task ThenReturnsTrueWhenUserIsEmployerUser(
        int ukprn,
        AccessCohortRequirement requirement,
        [Frozen] Mock<IHttpContextAccessor> httpContextAccessor,
        [Frozen] Mock<IReservationsOuterService> outerService,
        AccessCohortAuthorizationHelper sut)
    {
        var claimsPrinciple = new ClaimsPrincipal(new[]
        {
            new ClaimsIdentity(new[]
            {
                new Claim(EmployerClaims.AccountsClaimsTypeIdentifier, "AAACVVV")
            })
        });

        var httpContext = new DefaultHttpContext(new FeatureCollection()) { User = claimsPrinciple };
        httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        var actual = await sut.CanAccessCohort();

        actual.Should().BeTrue();
    }

    [Test, MoqAutoData]
    public async Task ThenReturnsFalseWhenUserIsProviderAndCohortRefIsMissingFromRoute(
        long ukprn,
        AccessCohortRequirement requirement,
        [Frozen] Mock<IHttpContextAccessor> httpContextAccessor,
        [Frozen] Mock<IReservationsOuterService> outerService,
        AccessCohortAuthorizationHelper sut)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.QueryString = new QueryString("?test=test");
        httpContext.Request.RouteValues.Add(RouteValueKeys.ProviderId, ukprn.ToString());
        httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        var actual = await sut.CanAccessCohort();

        actual.Should().BeFalse();
    }

    [Test, MoqAutoData]
    public async Task ThenThrowsExceptionWhenProviderIdCannotBeParsedToInteger(
        string ukprn,
        string cohortRef,
        AccessCohortRequirement requirement,
        [Frozen] Mock<IHttpContextAccessor> httpContextAccessor,
        AccessCohortAuthorizationHelper sut,
        GetAccountLegalEntitiesForProviderResponse response)
    {
        var claimsPrinciple = new ClaimsPrincipal(new[]
        {
            new ClaimsIdentity(new[]
            {
                new Claim(ProviderClaims.ProviderUkprn, ukprn)
            })
        });

        var httpContext = new DefaultHttpContext();
        httpContext.User = claimsPrinciple;
        httpContext.Request.QueryString = new QueryString($"?{RouteValueKeys.CohortReference}={cohortRef}");
        httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        var actual = async () => await sut.CanAccessCohort();

        await actual.Should()
            .ThrowAsync<ApplicationException>();
    }

    [Test, MoqAutoData]
    public async Task ThenCallsToCachedOuterApiWhenUserIsProviderAndCohortRefIsOnRoute(
        long ukprn,
        string cohortRef,
        long cohortId,
        AccessCohortRequirement requirement,
        [Frozen] Mock<IHttpContextAccessor> httpContextAccessor,
        [Frozen] Mock<ICachedReservationsOuterService> outerService,
        [Frozen] Mock<IEncodingService> encodingService)
    {
        var claimsPrinciple = new ClaimsPrincipal(new[]
        {
            new ClaimsIdentity(new[]
            {
                new Claim(ProviderClaims.ProviderUkprn, ukprn.ToString())
            })
        });

        var httpContext = new DefaultHttpContext();
        httpContext.User = claimsPrinciple;
        httpContext.Request.QueryString = new QueryString($"?{RouteValueKeys.CohortReference}={cohortRef}");
        httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);
        outerService.Setup(x => x.CanAccessCohort(ukprn, cohortId)).ReturnsAsync(true);
        encodingService.Setup(x => x.Decode(cohortRef, EncodingType.CohortReference)).Returns(cohortId);

        var sut = new AccessCohortAuthorizationHelper(outerService.Object, httpContextAccessor.Object, Mock.Of<ILogger<AccessCohortAuthorizationHelper>>(), encodingService.Object);

        var actual = await sut.CanAccessCohort();

        actual.Should().Be(true);

        outerService.Verify(x => x.CanAccessCohort(ukprn, cohortId), Times.Once);
    }
}