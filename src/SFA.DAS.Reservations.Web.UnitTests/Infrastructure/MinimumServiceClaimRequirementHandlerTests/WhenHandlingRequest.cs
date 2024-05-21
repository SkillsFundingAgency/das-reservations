using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Web.Infrastructure;
using SFA.DAS.Reservations.Web.Infrastructure.Authorization;

namespace SFA.DAS.Reservations.Web.UnitTests.Infrastructure.MinimumServiceClaimRequirementHandlerTests
{
    public class WhenHandlingRequest
    {
        [Test]
        public async Task If_The_User_Has_DAA_Claim_Then_Succeeds()
        {

            WhenHandlingRequestFixture fixture = new WhenHandlingRequestFixture();
            var context = fixture.SetWithProviderUserServiceClaims(new List<ServiceClaim> { ServiceClaim.DAA }, 
                new MinimumServiceClaimRequirement(ServiceClaim.DAA));

            //Act
            await fixture.Handle(context); ;

            //Assert
            context.HasSucceeded.Should().BeTrue(); 
        }

        [Test]
        public async Task If_The_User_Has_DAB_Claim_Then_The_Requirement_Is_For_DAA_Then_HasSucceeded_Is_False( )
        {

            WhenHandlingRequestFixture fixture = new WhenHandlingRequestFixture();
            var context = fixture.SetWithProviderUserServiceClaims(new List<ServiceClaim> { ServiceClaim.DAB },
                new MinimumServiceClaimRequirement(ServiceClaim.DAA));

            //Act
            await fixture.Handle(context); ;

            //Assert
            context.HasSucceeded.Should().BeFalse(); 
        }

        [Test]
        public async Task If_The_User_Is_Employer_The_Requirement_Is_By_Passed_And_HasSucceeded_Is_True()
        {
            WhenHandlingRequestFixture fixture = new WhenHandlingRequestFixture();
            var context = fixture.SetWithEmployer();

            //Act
            await fixture.Handle(context); 

            //Assert
            context.HasSucceeded.Should().BeTrue();
        }

        [TestCase(ServiceClaim.DAA, ServiceClaim.DAA, true)]
        [TestCase(ServiceClaim.DAA, ServiceClaim.DAB, true)]
        [TestCase(ServiceClaim.DAA, ServiceClaim.DAC, true)]
        [TestCase(ServiceClaim.DAA, ServiceClaim.DAV, true)]
        [TestCase(ServiceClaim.DAB, ServiceClaim.DAA, false)]
        [TestCase(ServiceClaim.DAB, ServiceClaim.DAB, true)]
        [TestCase(ServiceClaim.DAB, ServiceClaim.DAC, true)]
        [TestCase(ServiceClaim.DAB, ServiceClaim.DAV, true)]
        [TestCase(ServiceClaim.DAC, ServiceClaim.DAA, false)]
        [TestCase(ServiceClaim.DAC, ServiceClaim.DAB, false)]
        [TestCase(ServiceClaim.DAC, ServiceClaim.DAC, true)]
        [TestCase(ServiceClaim.DAC, ServiceClaim.DAV, true)]
        [TestCase(ServiceClaim.DAV, ServiceClaim.DAA, false)]
        [TestCase(ServiceClaim.DAV, ServiceClaim.DAB, false)]
        [TestCase(ServiceClaim.DAV, ServiceClaim.DAC, false)]
        [TestCase(ServiceClaim.DAV, ServiceClaim.DAV, true)]
        public async Task Verify_Minimum_Claim_Requirement(
         ServiceClaim userServiceClaim, ServiceClaim minimumRequirementClaim, bool hasSucceeded)
        {
            WhenHandlingRequestFixture fixture = new WhenHandlingRequestFixture();
            var context = fixture.SetWithProviderUserServiceClaims(new List<ServiceClaim> { userServiceClaim },
                new MinimumServiceClaimRequirement(minimumRequirementClaim));

            //Act
            await fixture.Handle(context);

            //Assert
            context.HasSucceeded.Should().Equals(hasSucceeded);
        }

        [Test]
        public async Task If_The_User_Has_Multiple_Service_Claim_Then_The_Highest_Claim_Is_Considered()
        {

            WhenHandlingRequestFixture fixture = new WhenHandlingRequestFixture();
            var context = fixture.SetWithProviderUserServiceClaims(new List<ServiceClaim> { ServiceClaim.DAV, ServiceClaim.DAC, ServiceClaim.DAB },
                new MinimumServiceClaimRequirement(ServiceClaim.DAB));

            //Act
            await fixture.Handle(context); ;

            //Assert
            context.HasSucceeded.Should().BeTrue();
        }

        public class WhenHandlingRequestFixture
        {
            MinimumServiceClaimRequirementHandler _handler;

            public WhenHandlingRequestFixture() 
            {
                _handler = new MinimumServiceClaimRequirementHandler(Mock.Of<ILogger<MinimumServiceClaimRequirementHandler>>());
            }

            public AuthorizationHandlerContext SetWithProviderUserServiceClaims(List<ServiceClaim> serviceClaims, MinimumServiceClaimRequirement requirement)
            {
                var claims = new List<Claim>();

                foreach (var claim in serviceClaims)
                {
                    claims.Add(new Claim(ProviderClaims.Service, claim.ToString()));
                }
                var author = "author";
                var user = new ClaimsPrincipal(
                            new ClaimsIdentity(
                              claims,
                                "Basic")
                            );

                var requirements = new[] { requirement };
                var context = new AuthorizationHandlerContext(requirements, user, author);

                return context;
            }

            public AuthorizationHandlerContext SetWithEmployer()
            {
                DefaultHttpContext fakeAuthFilterContext =
                    new DefaultHttpContext();
                fakeAuthFilterContext.Request.RouteValues.Add(RouteValues.EmployerAccountId, "somevalue");
                
                var claims = new List<Claim>();
                
                var user = new ClaimsPrincipal(
                            new ClaimsIdentity(
                              claims,
                                "Basic")
                            );

                var requirements = new[] { new MinimumServiceClaimRequirement(ServiceClaim.DAA) };
                var context = new AuthorizationHandlerContext(requirements, user, fakeAuthFilterContext);

                return context;
            }

            public async Task Handle(AuthorizationHandlerContext context)
            {
                await _handler.HandleAsync(context);
            }
        }
    }
}
