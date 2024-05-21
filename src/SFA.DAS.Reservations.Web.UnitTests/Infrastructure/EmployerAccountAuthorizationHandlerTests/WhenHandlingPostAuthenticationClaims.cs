using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Infrastructure.Configuration;
using SFA.DAS.Reservations.Infrastructure.Services;
using SFA.DAS.Reservations.Web.Infrastructure;
using SFA.DAS.Reservations.Web.Infrastructure.Authorization;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Reservations.Web.UnitTests.Infrastructure.EmployerAccountAuthorizationHandlerTests
{
    public class WhenHandlingPostAuthenticationClaims
    {
        [Test, MoqAutoData]
        public async Task Then_The_Claims_Are_Populated_For_Gov_User(
        string nameIdentifier,
        string idamsIdentifier,
        string emailAddress,
        string  claimValue,
        [Frozen] Mock<IEmployerAccountService> accountService,
        [Frozen] Mock<IOptions<ReservationsWebConfiguration>> apimWebConfiguration,
        EmployerAccountPostAuthenticationClaimsHandler handler)
        {
            var accountData = new Claim(EmployerClaims.AccountsClaimsTypeIdentifier, claimValue);
            apimWebConfiguration.Object.Value.UseGovSignIn = true;
            var tokenValidatedContext = ArrangeTokenValidatedContext(nameIdentifier, idamsIdentifier, emailAddress);
            accountService.Setup(x => x.GetClaim(nameIdentifier,EmployerClaims.AccountsClaimsTypeIdentifier,emailAddress)).ReturnsAsync(new List<Claim>{accountData});
            
            var actual = await handler.GetClaims(tokenValidatedContext);
            
            accountService.Verify(x=>x.GetClaim(nameIdentifier,EmployerClaims.AccountsClaimsTypeIdentifier,emailAddress), Times.Once);
            accountService.Verify(x=>x.GetClaim(idamsIdentifier,EmployerClaims.AccountsClaimsTypeIdentifier,emailAddress), Times.Never);
            actual.Should().ContainSingle(c => c.Type.Equals(EmployerClaims.AccountsClaimsTypeIdentifier));
            var actualClaimValue = actual.First(c => c.Type.Equals(EmployerClaims.AccountsClaimsTypeIdentifier));
            actualClaimValue.Should().Be(accountData);
            
        }

        [Test, MoqAutoData]
        public async Task Then_The_Claims_Are_Populated_For_EmployerUsers_User(
            string nameIdentifier,
            string idamsIdentifier,
            string  claimValue,
            [Frozen] Mock<IEmployerAccountService> accountService,
            [Frozen] Mock<IOptions<ReservationsWebConfiguration>> apimDeveloperWebConfiguration,
            EmployerAccountPostAuthenticationClaimsHandler handler)
        {
            var accountData = new Claim(EmployerClaims.AccountsClaimsTypeIdentifier, claimValue);
            var tokenValidatedContext = ArrangeTokenValidatedContext(nameIdentifier, idamsIdentifier, string.Empty);
            accountService.Setup(x => x.GetClaim(idamsIdentifier,EmployerClaims.AccountsClaimsTypeIdentifier, "")).ReturnsAsync(new List<Claim>{accountData});
            apimDeveloperWebConfiguration.Object.Value.UseGovSignIn = false;
            
            var actual = await handler.GetClaims(tokenValidatedContext);
            
            accountService.Verify(x=>x.GetClaim(nameIdentifier,EmployerClaims.AccountsClaimsTypeIdentifier, string.Empty), Times.Never);
            accountService.Verify(x=>x.GetClaim(idamsIdentifier,EmployerClaims.AccountsClaimsTypeIdentifier, string.Empty), Times.Once);
            actual.Should().ContainSingle(c => c.Type.Equals(EmployerClaims.AccountsClaimsTypeIdentifier));
            
            var actualClaimValue = actual.First(c => c.Type.Equals(EmployerClaims.AccountsClaimsTypeIdentifier)).Value;
            actual.First().Should().Be(accountData);
        }
        private TokenValidatedContext ArrangeTokenValidatedContext(string nameIdentifier, string idamsIdentifier, string emailAddress)
        {
            var identity = new ClaimsIdentity(new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, nameIdentifier),
                new Claim(EmployerClaims.IdamsUserIdClaimTypeIdentifier, idamsIdentifier),
                new Claim(ClaimTypes.Email, emailAddress)
            });
            
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(identity));
            return new TokenValidatedContext(new DefaultHttpContext(), new AuthenticationScheme(",","", typeof(TestAuthHandler)),
                new OpenIdConnectOptions(), Mock.Of<ClaimsPrincipal>(), new AuthenticationProperties())
            {
                Principal = claimsPrincipal
            };
        }
        
        private class TestAuthHandler : IAuthenticationHandler
        {
            public Task InitializeAsync(AuthenticationScheme scheme, HttpContext context)
            {
                throw new NotImplementedException();
            }

            public Task<AuthenticateResult> AuthenticateAsync()
            {
                throw new NotImplementedException();
            }

            public Task ChallengeAsync(AuthenticationProperties properties)
            {
                throw new NotImplementedException();
            }

            public Task ForbidAsync(AuthenticationProperties properties)
            {
                throw new NotImplementedException();
                
            }
        }
    }
}