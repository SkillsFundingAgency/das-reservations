// using System.Collections.Generic;
// using System.Linq;
// using System.Security.Claims;
// using System.Threading.Tasks;
// using AutoFixture.NUnit3;
// using FluentAssertions;
// using FluentAssertions.Execution;
// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.Http;
// using Microsoft.AspNetCore.Http.Features;
// using Microsoft.Extensions.Logging;
// using Moq;
// using Newtonsoft.Json;
// using NUnit.Framework;
// using SFA.DAS.DfESignIn.Auth.Extensions;
// using SFA.DAS.Encoding;
// using SFA.DAS.Reservations.Domain.Interfaces;
// using SFA.DAS.Reservations.Domain.Providers.Api;
// using SFA.DAS.Reservations.Web.Handlers;
// using SFA.DAS.Reservations.Web.Infrastructure;
// using SFA.DAS.Testing.AutoFixture;
//
// namespace SFA.DAS.Reservations.Web.UnitTests.Handlers.AccessCohortAuthorizationHelperTests;
//
// public class WhenDeterminingIsAuthorized
// {
//     [Test, MoqAutoData]
//     public async Task ThenReturnsFalseWhenAccountLegalEntityIdIsMissingFromRoute(
//         int ukprn,
//         AccessCohortRequirement requirement,
//         [Frozen] Mock<IHttpContextAccessor> httpContextAccessor,
//         [Frozen] Mock<IReservationsOuterService> outerService,
//         AccessCohortAuthorizationHelper sut)
//     {
//         var claimsPrinciple = new ClaimsPrincipal(new[]
//         {
//             new ClaimsIdentity(new[]
//             {
//                 new Claim(ClaimsIdentity.DefaultNameClaimType, ukprn.ToString())
//             })
//         });
//
//         var context = new AuthorizationHandlerContext(new[] { requirement }, claimsPrinciple, null);
//         var httpContext = new DefaultHttpContext(new FeatureCollection());
//         httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);
//
//         var actual = await sut.IsAuthorised();
//
//         actual.Should().BeFalse();
//     }
//
//     [Test, MoqAutoData]
//     public async Task ThenReturnsFalseWhenAccountLegalEntityIdIsEmptyFromRoute(
//         int ukprn,
//         AccessCohortRequirement requirement,
//         [Frozen] Mock<IHttpContextAccessor> httpContextAccessor,
//         [Frozen] Mock<IReservationsOuterService> outerService,
//         AccessCohortAuthorizationHelper sut)
//     {
//         var claimsPrinciple = new ClaimsPrincipal(new[]
//         {
//             new ClaimsIdentity(new[]
//             {
//                 new Claim(ClaimsIdentity.DefaultNameClaimType, ukprn.ToString())
//             })
//         });
//
//         var context = new AuthorizationHandlerContext(new[] { requirement }, claimsPrinciple, null);
//         var httpContext = new DefaultHttpContext(new FeatureCollection());
//
//         httpContext.Request.RouteValues.Add(RouteValueKeys.AccountLegalEntityPublicHashedId, string.Empty);
//         httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);
//
//         var actual = await sut.IsProviderAuthorised();
//
//         actual.Should().BeFalse();
//     }
//
//     [Test, MoqAutoData]
//     public async Task ThenCallsToOuterApiWhenTrustedEmployersClaimIsEmptyAndSavesResultToClaims(
//         int ukprn,
//         string accountLegalEntityHashedId,
//         AccessCohortRequirement requirement,
//         [Frozen] Mock<IHttpContextAccessor> httpContextAccessor,
//         [Frozen] Mock<IReservationsOuterService> outerService,
//         AccessCohortAuthorizationHelper sut,
//         GetAccountProviderLegalEntitiesWithCreateCohortResponse response)
//     {
//         var claimsPrinciple = new ClaimsPrincipal(new[]
//         {
//             new ClaimsIdentity(new[]
//             {
//                 new Claim(ProviderClaims.ProviderUkprn, ukprn.ToString())
//             })
//         });
//
//         var context = new AuthorizationHandlerContext(new[] { requirement }, claimsPrinciple, null);
//         var httpContext = new DefaultHttpContext(new FeatureCollection());
//
//         httpContext.Request.RouteValues.Add(RouteValueKeys.AccountLegalEntityPublicHashedId, accountLegalEntityHashedId);
//         httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);
//         outerService.Setup(x => x.GetAccountProviderLegalEntitiesWithCreateCohort(ukprn)).ReturnsAsync(response);
//
//         var actual = await sut.IsProviderAuthorised();
//
//         actual.Should().BeFalse();
//
//         outerService.Verify(x => x.GetAccountProviderLegalEntitiesWithCreateCohort(ukprn), Times.Once);
//
//         var claimResult = claimsPrinciple.GetClaimValue(ProviderClaims.TrustedEmployerAccounts);
//
//         using (new AssertionScope())
//         {
//             claimResult.Should().NotBeEmpty();
//             claimResult.Should().Be(JsonConvert.SerializeObject(response.AccountProviderLegalEntities.ToDictionary(x => x.AccountId)));
//         }
//     }
//
//     [Test, MoqAutoData]
//     public async Task ThenDoesNotCallToOuterApiWhenTrustedEmployersClaimIsPopulated(
//         int ukprn,
//         string accountLegalEntityHashedId,
//         AccessCohortRequirement requirement,
//         [Frozen] Mock<IHttpContextAccessor> httpContextAccessor,
//         [Frozen] Mock<IReservationsOuterService> outerService,
//         AccessCohortAuthorizationHelper sut,
//         GetAccountProviderLegalEntitiesWithCreateCohortResponse response)
//     {
//         var claimsPrinciple = new ClaimsPrincipal(new[]
//         {
//             new ClaimsIdentity(new[]
//             {
//                 new Claim(ClaimsIdentity.DefaultNameClaimType, ukprn.ToString()),
//                 new Claim(ProviderClaims.TrustedEmployerAccounts, JsonConvert.SerializeObject(response.AccountProviderLegalEntities.ToDictionary(x => x.AccountId)))
//             })
//         });
//
//         var context = new AuthorizationHandlerContext(new[] { requirement }, claimsPrinciple, null);
//         var httpContext = new DefaultHttpContext(new FeatureCollection());
//
//         httpContext.Request.RouteValues.Add(RouteValueKeys.AccountLegalEntityPublicHashedId, accountLegalEntityHashedId);
//         httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);
//         outerService.Setup(x => x.GetAccountProviderLegalEntitiesWithCreateCohort(ukprn)).ReturnsAsync(response);
//
//         var actual = await sut.IsProviderAuthorised();
//
//         using (new AssertionScope())
//         {
//             actual.Should().BeFalse();
//
//             outerService.Verify(x => x.GetAccountProviderLegalEntitiesWithCreateCohort(ukprn), Times.Never);
//
//             var claimResult = claimsPrinciple.GetClaimValue(ProviderClaims.TrustedEmployerAccounts);
//
//             claimResult.Should().Be(JsonConvert.SerializeObject(response.AccountProviderLegalEntities.ToDictionary(x => x.AccountId)));
//         }
//     }
//
//     [Test, MoqAutoData]
//     public async Task ThenReturnsFalseWhenAccountLegalEntityIdIsNotInTrustedAccounts(
//         int ukprn,
//         string accountLegalEntityHashedId,
//         long accountLegalEntityId,
//         AccessCohortRequirement requirement,
//         [Frozen] Mock<IHttpContextAccessor> httpContextAccessor,
//         [Frozen] Mock<IReservationsOuterService> outerService,
//         AccessCohortAuthorizationHelper sut,
//         GetAccountProviderLegalEntitiesWithCreateCohortResponse response,
//         [Frozen] Mock<IEncodingService> encodingService)
//     {
//         var claimsPrinciple = new ClaimsPrincipal(new[]
//         {
//             new ClaimsIdentity(new[]
//             {
//                 new Claim(ClaimsIdentity.DefaultNameClaimType, ukprn.ToString()),
//                 new Claim(ProviderClaims.TrustedEmployerAccounts, JsonConvert.SerializeObject(response.AccountProviderLegalEntities.ToDictionary(x => x.AccountId)))
//             })
//         });
//
//         var context = new AuthorizationHandlerContext(new[] { requirement }, claimsPrinciple, null);
//         var httpContext = new DefaultHttpContext(new FeatureCollection());
//
//         httpContext.Request.RouteValues.Add(RouteValueKeys.AccountLegalEntityPublicHashedId, accountLegalEntityHashedId);
//         httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);
//         outerService.Setup(x => x.GetAccountProviderLegalEntitiesWithCreateCohort(ukprn)).ReturnsAsync(response);
//         encodingService.Setup(x => x.Decode(accountLegalEntityHashedId, EncodingType.AccountLegalEntityId)).Returns(accountLegalEntityId);
//
//         var actual = await sut.IsProviderAuthorised();
//
//         actual.Should().BeFalse();
//     }
//
//     [Test, MoqAutoData]
//     public async Task ThenReturnsTrueWhenAccountLegalEntityIdIsInTrustedAccounts(
//         int ukprn,
//         string accountLegalEntityHashedId,
//         long accountLegalEntityId,
//         AccessCohortRequirement requirement,
//         [Frozen] Mock<IHttpContextAccessor> httpContextAccessor,
//         [Frozen] Mock<IReservationsOuterService> outerService,
//         List<GetAccountProviderLegalEntitiesWithCreateCohortResponse.AccountProviderLegalEntityDto> trustedAccounts,
//         [Frozen] Mock<IEncodingService> encodingService)
//     {
//         trustedAccounts.Add(new GetAccountProviderLegalEntitiesWithCreateCohortResponse.AccountProviderLegalEntityDto { AccountId = accountLegalEntityId });
//
//         var response = new GetAccountProviderLegalEntitiesWithCreateCohortResponse
//         {
//             AccountProviderLegalEntities = trustedAccounts,
//         };
//
//         var claimsPrinciple = new ClaimsPrincipal(new[]
//         {
//             new ClaimsIdentity(new[]
//             {
//                 new Claim(ClaimsIdentity.DefaultNameClaimType, ukprn.ToString()),
//                 new Claim(ProviderClaims.TrustedEmployerAccounts, JsonConvert.SerializeObject(response.AccountProviderLegalEntities.ToDictionary(x => x.AccountId)))
//             })
//         });
//
//         var context = new AuthorizationHandlerContext(new[] { requirement }, claimsPrinciple, null);
//         var httpContext = new DefaultHttpContext(new FeatureCollection());
//
//         httpContext.Request.RouteValues.Add(RouteValueKeys.AccountLegalEntityPublicHashedId, accountLegalEntityHashedId);
//         httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);
//         outerService.Setup(x => x.GetAccountProviderLegalEntitiesWithCreateCohort(ukprn)).ReturnsAsync(response);
//         encodingService.Setup(x => x.Decode(accountLegalEntityHashedId, EncodingType.AccountLegalEntityId)).Returns(accountLegalEntityId);
//
//         var sut = new AccessCohortAuthorizationHelper(httpContextAccessor.Object, Mock.Of<ILogger<AccessCohortAuthorizationHandler>>(), encodingService.Object, outerService.Object);
//
//         var actual = await sut.IsProviderAuthorised();
//
//         using (new AssertionScope())
//         {
//             encodingService.Verify(x => x.Decode(It.Is<string>(y => y.Equals(accountLegalEntityHashedId)), EncodingType.AccountLegalEntityId), Times.Once);
//             actual.Should().BeTrue();
//         }
//     }
// }