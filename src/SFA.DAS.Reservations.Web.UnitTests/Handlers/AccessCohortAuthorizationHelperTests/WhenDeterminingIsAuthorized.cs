using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using SFA.DAS.DfESignIn.Auth.Extensions;
using SFA.DAS.Encoding;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Domain.Providers.Api;
using SFA.DAS.Reservations.Infrastructure.Services;
using SFA.DAS.Reservations.Web.Handlers;
using SFA.DAS.Reservations.Web.Infrastructure;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Reservations.Web.UnitTests.Handlers.AccessCohortAuthorizationHelperTests;

public class WhenDeterminingIsAuthorized
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

        var actual = await sut.IsAuthorised();

        actual.Should().BeFalse();
    }

    [Test, MoqAutoData]
    public async Task ThenReturnsFalseWhenUserIsProviderAndAccountLegalEntityIdIsMissingFromRoute(
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
                new Claim(ClaimsIdentity.DefaultNameClaimType, ukprn.ToString())
            })
        });

        var httpContext = new DefaultHttpContext(new FeatureCollection()) { User = claimsPrinciple };
        httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        var actual = await sut.IsAuthorised();

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

        var actual = await sut.IsAuthorised();

        actual.Should().BeTrue();
    }

    [Test, MoqAutoData]
    public async Task ThenReturnsFalseWhenUserIsProviderAndAccountLegalEntityIdIsEmptyFromRoute(
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
                new Claim(ClaimsIdentity.DefaultNameClaimType, ukprn.ToString())
            })
        });

        var httpContext = new DefaultHttpContext(new FeatureCollection()) { User = claimsPrinciple };

        httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        var actual = await sut.IsAuthorised();

        actual.Should().BeFalse();
    }

    [Test, MoqAutoData]
    public async Task ThenCallsToOuterApiWhenUserIsProviderAndTrustedEmployersClaimIsEmptyAndSavesResultToClaims(
        int ukprn,
        string accountLegalEntityHashedId,
        AccessCohortRequirement requirement,
        [Frozen] Mock<IHttpContextAccessor> httpContextAccessor,
        [Frozen] Mock<IReservationsOuterService> outerService,
        AccessCohortAuthorizationHelper sut,
        GetAccountProviderLegalEntitiesWithCreateCohortResponse response)
    {
        var claimsPrinciple = new ClaimsPrincipal(new[]
        {
            new ClaimsIdentity(new[]
            {
                new Claim(ProviderClaims.ProviderUkprn, ukprn.ToString())
            })
        });

        var httpContext = new DefaultHttpContext(new FeatureCollection()) { User = claimsPrinciple };

        httpContext.Request.RouteValues.Add(RouteValueKeys.AccountLegalEntityPublicHashedId, accountLegalEntityHashedId);
        httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);
        outerService.Setup(x => x.GetAccountProviderLegalEntitiesWithCreateCohort(ukprn)).ReturnsAsync(response);

        var actual = await sut.IsAuthorised();

        actual.Should().BeFalse();

        outerService.Verify(x => x.GetAccountProviderLegalEntitiesWithCreateCohort(ukprn), Times.Once);

        var claimResult = claimsPrinciple.GetClaimValue(ProviderClaims.AssociatedAccountsClaimsTypeIdentifier);

        using (new AssertionScope())
        {
            claimResult.Should().NotBeEmpty();
            claimResult.Should().Be(JsonConvert.SerializeObject(response.AccountProviderLegalEntities.ToDictionary(x => x.AccountId)));
        }
    }
    
    [Test, MoqAutoData]
    public async Task ThenCallsToOuterApiWhenUserIsProviderAndTrustedEmployersClaimIsEmptyAndSavesResultToClaimsWhenThereAreDuplicateAccountIds(
        int ukprn,
        string accountLegalEntityHashedId,
        AccessCohortRequirement requirement,
        [Frozen] Mock<IHttpContextAccessor> httpContextAccessor,
        [Frozen] Mock<IReservationsOuterService> outerService,
        AccessCohortAuthorizationHelper sut,
        GetAccountProviderLegalEntitiesWithCreateCohortResponse response)
    {
        var claimsPrinciple = new ClaimsPrincipal(new[]
        {
            new ClaimsIdentity(new[]
            {
                new Claim(ProviderClaims.ProviderUkprn, ukprn.ToString())
            })
        });
        
        response.AccountProviderLegalEntities.Add(new GetProviderAccountLegalEntityWithCreatCohortItem{ AccountId = 111});
        response.AccountProviderLegalEntities.Add(new GetProviderAccountLegalEntityWithCreatCohortItem{ AccountId = 111});

        var httpContext = new DefaultHttpContext(new FeatureCollection()) { User = claimsPrinciple };

        httpContext.Request.RouteValues.Add(RouteValueKeys.AccountLegalEntityPublicHashedId, accountLegalEntityHashedId);
        httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);
        outerService.Setup(x => x.GetAccountProviderLegalEntitiesWithCreateCohort(ukprn)).ReturnsAsync(response);

        var actual = await sut.IsAuthorised();

        actual.Should().BeFalse();

        outerService.Verify(x => x.GetAccountProviderLegalEntitiesWithCreateCohort(ukprn), Times.Once);

        var claimResult = claimsPrinciple.GetClaimValue(ProviderClaims.AssociatedAccountsClaimsTypeIdentifier);

        using (new AssertionScope())
        {
            claimResult.Should().NotBeEmpty();
            claimResult.Should().Be(JsonConvert.SerializeObject(response.AccountProviderLegalEntities.DistinctBy(x=> x.AccountId).ToDictionary(x => x.AccountId)));
        }
    }

    [Test, MoqAutoData]
    public async Task ThenDoesNotCallToOuterApiWhenUserIsProviderAndTrustedEmployersClaimIsPopulated(
        int ukprn,
        string accountLegalEntityHashedId,
        AccessCohortRequirement requirement,
        [Frozen] Mock<IHttpContextAccessor> httpContextAccessor,
        [Frozen] Mock<IReservationsOuterService> outerService,
        AccessCohortAuthorizationHelper sut,
        GetAccountProviderLegalEntitiesWithCreateCohortResponse response)
    {
        var claimsPrinciple = new ClaimsPrincipal(new[]
        {
            new ClaimsIdentity(new[]
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, ukprn.ToString()),
                new Claim(ProviderClaims.AssociatedAccountsClaimsTypeIdentifier, JsonConvert.SerializeObject(response.AccountProviderLegalEntities.ToDictionary(x => x.AccountId)))
            })
        });

        var httpContext = new DefaultHttpContext(new FeatureCollection()) { User = claimsPrinciple };

        httpContext.Request.RouteValues.Add(RouteValueKeys.AccountLegalEntityPublicHashedId, accountLegalEntityHashedId);
        httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);
        outerService.Setup(x => x.GetAccountProviderLegalEntitiesWithCreateCohort(ukprn)).ReturnsAsync(response);

        var actual = await sut.IsAuthorised();

        using (new AssertionScope())
        {
            actual.Should().BeFalse();

            outerService.Verify(x => x.GetAccountProviderLegalEntitiesWithCreateCohort(ukprn), Times.Never);

            var claimResult = claimsPrinciple.GetClaimValue(ProviderClaims.AssociatedAccountsClaimsTypeIdentifier);

            claimResult.Should().Be(JsonConvert.SerializeObject(response.AccountProviderLegalEntities.ToDictionary(x => x.AccountId)));
        }
    }

    [Test, MoqAutoData]
    public async Task ThenReturnsFalseWhenUserIsProviderAndAccountLegalEntityIdIsNotInTrustedAccounts(
        int ukprn,
        string accountLegalEntityHashedId,
        long accountLegalEntityId,
        AccessCohortRequirement requirement,
        [Frozen] Mock<IHttpContextAccessor> httpContextAccessor,
        [Frozen] Mock<IReservationsOuterService> outerService,
        AccessCohortAuthorizationHelper sut,
        GetAccountProviderLegalEntitiesWithCreateCohortResponse response,
        [Frozen] Mock<IEncodingService> encodingService)
    {
        var claimsPrinciple = new ClaimsPrincipal(new[]
        {
            new ClaimsIdentity(new[]
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, ukprn.ToString()),
                new Claim(ProviderClaims.AssociatedAccountsClaimsTypeIdentifier, JsonConvert.SerializeObject(response.AccountProviderLegalEntities.ToDictionary(x => x.AccountId)))
            })
        });

        var httpContext = new DefaultHttpContext(new FeatureCollection()) { User = claimsPrinciple };

        httpContext.Request.RouteValues.Add(RouteValueKeys.AccountLegalEntityPublicHashedId, accountLegalEntityHashedId);
        httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);
        outerService.Setup(x => x.GetAccountProviderLegalEntitiesWithCreateCohort(ukprn)).ReturnsAsync(response);
        encodingService.Setup(x => x.Decode(accountLegalEntityHashedId, EncodingType.PublicAccountLegalEntityId)).Returns(accountLegalEntityId);

        var actual = await sut.IsAuthorised();

        actual.Should().BeFalse();
    }

    [Test, MoqAutoData]
    public async Task ThenReturnsTrueWhenUserIsProviderAndAccountLegalEntityIdIsInTrustedAccounts(
        int ukprn,
        string accountLegalEntityHashedId,
        long accountLegalEntityId,
        AccessCohortRequirement requirement,
        [Frozen] Mock<IHttpContextAccessor> httpContextAccessor,
        [Frozen] Mock<IReservationsOuterService> outerService,
        List<GetProviderAccountLegalEntityWithCreatCohortItem> trustedAccounts,
        [Frozen] Mock<IEncodingService> encodingService)
    {
        trustedAccounts.Add(new GetProviderAccountLegalEntityWithCreatCohortItem { AccountId = accountLegalEntityId });

        var response = new GetAccountProviderLegalEntitiesWithCreateCohortResponse
        {
            AccountProviderLegalEntities = trustedAccounts,
        };

        var claimsPrinciple = new ClaimsPrincipal(new[]
        {
            new ClaimsIdentity(new[]
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, ukprn.ToString()),
                new Claim(ProviderClaims.AssociatedAccountsClaimsTypeIdentifier, JsonConvert.SerializeObject(response.AccountProviderLegalEntities.ToDictionary(x => x.AccountId)))
            })
        });

        var httpContext = new DefaultHttpContext(new FeatureCollection()) { User = claimsPrinciple };

        httpContext.Request.RouteValues.Add(RouteValueKeys.AccountLegalEntityPublicHashedId, accountLegalEntityHashedId);
        httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);
        outerService.Setup(x => x.GetAccountProviderLegalEntitiesWithCreateCohort(ukprn)).ReturnsAsync(response);
        encodingService.Setup(x => x.Decode(accountLegalEntityHashedId, EncodingType.PublicAccountLegalEntityId)).Returns(accountLegalEntityId);

        var sut = new AccessCohortAuthorizationHelper(Mock.Of<ILogger<AccessCohortAuthorizationHelper>>(), httpContextAccessor.Object, encodingService.Object, outerService.Object);

        var actual = await sut.IsAuthorised();

        using (new AssertionScope())
        {
            encodingService.Verify(x => x.Decode(It.Is<string>(y => y.Equals(accountLegalEntityHashedId)), EncodingType.PublicAccountLegalEntityId), Times.Once);
            actual.Should().BeTrue();
        }
    }
}