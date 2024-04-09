using System;
using System.Collections.Generic;
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

    // [Test, MoqAutoData]
    // public async Task ThenReturnsFalseWhenUserIsProviderAndCohortRefIsMissingFromRoute(
    //     long ukprn,
    //     AccessCohortRequirement requirement,
    //     [Frozen] Mock<IHttpContextAccessor> httpContextAccessor,
    //     [Frozen] Mock<IReservationsOuterService> outerService,
    //     AccessCohortAuthorizationHelper sut)
    // {
    //     var claimsPrinciple = new ClaimsPrincipal(new[]
    //     {
    //         new ClaimsIdentity(new[]
    //         {
    //             new Claim(ClaimsIdentity.DefaultNameClaimType, ukprn.ToString())
    //         })
    //     });
    //
    //     var httpContext = new DefaultHttpContext(new FeatureCollection()) { User = claimsPrinciple, Request =
    //     {
    //         QueryString = new QueryString("?me=you")
    //     }};
    //     httpContext.Request.RouteValues.Add(RouteValueKeys.ProviderId, ukprn.ToString());
    //     httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);
    //
    //     var actual = await sut.CanAccessCohort();
    //
    //     actual.Should().BeFalse();
    // }
    
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
    
    // [Test, MoqAutoData]
    // public async Task ThenReturnsFalseWhenUserIsProviderAndAccountLegalEntityIdIsEmptyFromRoute(
    //     int ukprn,
    //     AccessCohortRequirement requirement,
    //     [Frozen] Mock<IHttpContextAccessor> httpContextAccessor,
    //     [Frozen] Mock<IReservationsOuterService> outerService,
    //     AccessCohortAuthorizationHelper sut)
    // {
    //     var claimsPrinciple = new ClaimsPrincipal(new[]
    //     {
    //         new ClaimsIdentity(new[]
    //         {
    //             new Claim(ClaimsIdentity.DefaultNameClaimType, ukprn.ToString())
    //         })
    //     });
    //
    //     var httpContext = new DefaultHttpContext(new FeatureCollection()) { User = claimsPrinciple };
    //
    //     httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);
    //
    //     var actual = await sut.CanCreateCohort();
    //
    //     actual.Should().BeFalse();
    // }
    //
    // [Test, MoqAutoData]
    // public async Task ThenThrowsExceptionWhenProviderIdCannotBeParsedToInteger(
    //     string accountLegalEntityHashedId,
    //     AccessCohortRequirement requirement,
    //     [Frozen] Mock<IHttpContextAccessor> httpContextAccessor,
    //     AccessCohortAuthorizationHelper sut,
    //     GetAccountLegalEntitiesForProviderResponse response)
    // {
    //     const string ukprn = "100999A";
    //
    //     var claimsPrinciple = new ClaimsPrincipal(new[]
    //     {
    //         new ClaimsIdentity(new[]
    //         {
    //             new Claim(ProviderClaims.ProviderUkprn, ukprn)
    //         })
    //     });
    //
    //     var httpContext = new DefaultHttpContext(new FeatureCollection()) { User = claimsPrinciple };
    //
    //     httpContext.Request.RouteValues.Add(RouteValueKeys.AccountLegalEntityPublicHashedId, accountLegalEntityHashedId);
    //     httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);
    //
    //     var actual = async () => await sut.CanCreateCohort();
    //
    //     await actual.Should()
    //         .ThrowAsync<ApplicationException>()
    //         .WithMessage($"{nameof(CreateCohortAuthorizationHelper)} Unable to parse providerId from ukprn claim value: {ukprn}.");
    // }
    //
    // [Test, MoqAutoData]
    // public async Task ThenCallsToOuterApiWhenUserIsProviderAndTrustedEmployersClaimIsEmptyAndSavesResultToClaims(
    //     int ukprn,
    //     string accountLegalEntityHashedId,
    //     AccessCohortRequirement requirement,
    //     [Frozen] Mock<IHttpContextAccessor> httpContextAccessor,
    //     [Frozen] Mock<IReservationsOuterService> outerService,
    //     AccessCohortAuthorizationHelper sut,
    //     GetAccountLegalEntitiesForProviderResponse response)
    // {
    //     var claimsPrinciple = new ClaimsPrincipal(new[]
    //     {
    //         new ClaimsIdentity(new[]
    //         {
    //             new Claim(ProviderClaims.ProviderUkprn, ukprn.ToString())
    //         })
    //     });
    //
    //     var httpContext = new DefaultHttpContext(new FeatureCollection()) { User = claimsPrinciple };
    //
    //     httpContext.Request.RouteValues.Add(RouteValueKeys.AccountLegalEntityPublicHashedId, accountLegalEntityHashedId);
    //     httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);
    //     outerService.Setup(x => x.GetAccountProviderLegalEntitiesWithCreateCohort(ukprn)).ReturnsAsync(response);
    //
    //     var actual = await sut.CanCreateCohort();
    //
    //     actual.Should().BeFalse();
    //
    //     outerService.Verify(x => x.GetAccountProviderLegalEntitiesWithCreateCohort(ukprn), Times.Once);
    //
    //     var claimResult = claimsPrinciple.GetClaimValue(ProviderClaims.TrustedAccounts);
    //
    //     using (new AssertionScope())
    //     {
    //         claimResult.Should().NotBeEmpty();
    //         claimResult.Should().Be(JsonConvert.SerializeObject(response.AccountProviderLegalEntities));
    //     }
    // }
    //
    // [Test, MoqAutoData]
    // public async Task ThenDoesNotCallToOuterApiWhenUserIsProviderAndTrustedEmployersClaimIsPopulated(
    //     int ukprn,
    //     string accountLegalEntityHashedId,
    //     AccessCohortRequirement requirement,
    //     [Frozen] Mock<IHttpContextAccessor> httpContextAccessor,
    //     [Frozen] Mock<IReservationsOuterService> outerService,
    //     AccessCohortAuthorizationHelper sut,
    //     GetAccountLegalEntitiesForProviderResponse response)
    // {
    //     var claimsPrinciple = new ClaimsPrincipal(new[]
    //     {
    //         new ClaimsIdentity(new[]
    //         {
    //             new Claim(ClaimsIdentity.DefaultNameClaimType, ukprn.ToString()),
    //             new Claim(ProviderClaims.TrustedAccounts, JsonConvert.SerializeObject(response.AccountProviderLegalEntities))
    //         })
    //     });
    //
    //     var httpContext = new DefaultHttpContext(new FeatureCollection()) { User = claimsPrinciple };
    //
    //     httpContext.Request.RouteValues.Add(RouteValueKeys.AccountLegalEntityPublicHashedId, accountLegalEntityHashedId);
    //     httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);
    //     outerService.Setup(x => x.GetAccountProviderLegalEntitiesWithCreateCohort(ukprn)).ReturnsAsync(response);
    //
    //     var actual = await sut.CanCreateCohort();
    //
    //     using (new AssertionScope())
    //     {
    //         actual.Should().BeFalse();
    //
    //         outerService.Verify(x => x.GetAccountProviderLegalEntitiesWithCreateCohort(ukprn), Times.Never);
    //
    //         var claimResult = claimsPrinciple.GetClaimValue(ProviderClaims.TrustedAccounts);
    //
    //         claimResult.Should().Be(JsonConvert.SerializeObject(response.AccountProviderLegalEntities));
    //     }
    // }
    //
    // [Test, MoqAutoData]
    // public async Task ThenReturnsFalseWhenUserIsProviderAndAccountLegalEntityIdIsNotInTrustedAccounts(
    //     int ukprn,
    //     string accountLegalEntityHashedId,
    //     long accountLegalEntityId,
    //     AccessCohortRequirement requirement,
    //     [Frozen] Mock<IHttpContextAccessor> httpContextAccessor,
    //     [Frozen] Mock<IReservationsOuterService> outerService,
    //     AccessCohortAuthorizationHelper sut,
    //     GetAccountLegalEntitiesForProviderResponse response,
    //     [Frozen] Mock<IEncodingService> encodingService)
    // {
    //     var claimsPrinciple = new ClaimsPrincipal(new[]
    //     {
    //         new ClaimsIdentity(new[]
    //         {
    //             new Claim(ClaimsIdentity.DefaultNameClaimType, ukprn.ToString()),
    //             new Claim(ProviderClaims.TrustedAccounts, JsonConvert.SerializeObject(response.AccountProviderLegalEntities))
    //         })
    //     });
    //
    //     var httpContext = new DefaultHttpContext(new FeatureCollection()) { User = claimsPrinciple };
    //
    //     httpContext.Request.RouteValues.Add(RouteValueKeys.AccountLegalEntityPublicHashedId, accountLegalEntityHashedId);
    //     httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);
    //     outerService.Setup(x => x.GetAccountProviderLegalEntitiesWithCreateCohort(ukprn)).ReturnsAsync(response);
    //     encodingService.Setup(x => x.Decode(accountLegalEntityHashedId, EncodingType.PublicAccountLegalEntityId)).Returns(accountLegalEntityId);
    //
    //     var actual = await sut.CanCreateCohort();
    //
    //     actual.Should().BeFalse();
    // }
    //
    // [Test, MoqAutoData]
    // public async Task ThenReturnsTrueWhenUserIsProviderAndAccountLegalEntityIdIsInTrustedAccounts(
    //     int ukprn,
    //     string accountLegalEntityHashedId,
    //     long accountLegalEntityId,
    //     AccessCohortRequirement requirement,
    //     [Frozen] Mock<IHttpContextAccessor> httpContextAccessor,
    //     [Frozen] Mock<IReservationsOuterService> outerService,
    //     List<GetAccountLegalEntitiesForProviderItem> trustedAccounts,
    //     [Frozen] Mock<IEncodingService> encodingService)
    // {
    //     trustedAccounts.Add(new GetAccountLegalEntitiesForProviderItem { AccountLegalEntityId = accountLegalEntityId });
    //
    //     var response = new GetAccountLegalEntitiesForProviderResponse
    //     {
    //         AccountProviderLegalEntities = trustedAccounts,
    //     };
    //
    //     var claimsPrinciple = new ClaimsPrincipal(new[]
    //     {
    //         new ClaimsIdentity(new[]
    //         {
    //             new Claim(ClaimsIdentity.DefaultNameClaimType, ukprn.ToString()),
    //             new Claim(ProviderClaims.TrustedAccounts, JsonConvert.SerializeObject(response.AccountProviderLegalEntities))
    //         })
    //     });
    //
    //     var httpContext = new DefaultHttpContext(new FeatureCollection()) { User = claimsPrinciple };
    //
    //     httpContext.Request.RouteValues.Add(RouteValueKeys.AccountLegalEntityPublicHashedId, accountLegalEntityHashedId);
    //     httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);
    //     outerService.Setup(x => x.GetAccountProviderLegalEntitiesWithCreateCohort(ukprn)).ReturnsAsync(response);
    //     encodingService.Setup(x => x.Decode(accountLegalEntityHashedId, EncodingType.PublicAccountLegalEntityId)).Returns(accountLegalEntityId);
    //
    //     var sut = new AccessCohortAuthorizationHelper(Mock.Of<ILogger<AccessCohortAuthorizationHelper>>(), httpContextAccessor.Object, encodingService.Object, outerService.Object);
    //
    //     var actual = await sut.CanCreateCohort();
    //
    //     using (new AssertionScope())
    //     {
    //         encodingService.Verify(x => x.Decode(It.Is<string>(y => y.Equals(accountLegalEntityHashedId)), EncodingType.PublicAccountLegalEntityId), Times.Once);
    //         actual.Should().BeTrue();
    //     }
    // }
}