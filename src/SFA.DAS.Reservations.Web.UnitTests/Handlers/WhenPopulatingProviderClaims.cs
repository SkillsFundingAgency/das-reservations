using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Domain.Providers.Api;
using SFA.DAS.Reservations.Web.Handlers;
using SFA.DAS.Reservations.Web.Infrastructure;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Reservations.Web.UnitTests.Handlers;

public class WhenPopulatingProviderClaims
{
    [Test, MoqAutoData]
    public async Task Then_The_Claims_Are_Populated_For_Provider_User(
        long ukprn,
        string displayName,
        [Frozen] Mock<HttpContext> httpContext,
        [Frozen] Mock<IReservationsOuterService> outerService,
        List<GetAccountProviderLegalEntitiesWithCreateCohortResponse.AccountProviderLegalEntityDto> accountLegalEntities,
        ProviderAccountPostAuthenticationClaimsHandler handler)
    {
        var identity = new Mock<ClaimsIdentity>();
        identity.Setup(id => id.Claims).Returns(new List<Claim>
        {
            new(ProviderClaims.ProviderUkprn, ukprn.ToString()),
            new(ProviderClaims.DisplayName, displayName)
        });

        outerService
            .Setup(x => x.GetAccountProviderLegalEntitiesWithCreateCohort(ukprn))
            .ReturnsAsync(new GetAccountProviderLegalEntitiesWithCreateCohortResponse { AccountProviderLegalEntities = accountLegalEntities });

        var principal = new ClaimsPrincipal();
        principal.AddIdentity(identity.Object);

        var actual = await handler.GetClaims(httpContext.Object, principal);
        outerService.Verify(x => x.GetAccountProviderLegalEntitiesWithCreateCohort(ukprn), Times.Once);

        actual.Count().Should().Be(3);

        var actualClaimValue = actual.First(c => c.Type.Equals(ProviderClaims.TrustedEmployerAccounts)).Value;
        JsonConvert.SerializeObject(accountLegalEntities.ToDictionary(x => x.AccountId)).Should().Be(actualClaimValue);

        actual.First(c => c.Type.Equals(ClaimsIdentity.DefaultNameClaimType)).Value.Should().Be(ukprn.ToString());
        actual.First(c => c.Type.Equals(ProviderClaims.DisplayName)).Value.Should().Be(displayName);
    }
}