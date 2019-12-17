using System.Collections.Generic;
using System.Security.Claims;
using AutoFixture.NUnit3;
using FluentAssertions;
using Newtonsoft.Json;
using NUnit.Framework;
using SFA.DAS.Reservations.Domain.Authentication;
using SFA.DAS.Reservations.Domain.Employers;
using SFA.DAS.Reservations.Web.Infrastructure;
using SFA.DAS.Reservations.Web.Services;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Reservations.Web.UnitTests.Services
{
    public class WhenCallingUserIsInRole
    {
        [Test, MoqAutoData]
        public void And_User_Not_In_Role_Then_Returns_False(
            string employerAccountId,
            UserClaimsService userClaimsService)
        {
            var claims = BuildClaims(employerAccountId, EmployerUserRole.None);

            userClaimsService.UserIsInRole(employerAccountId, EmployerUserRole.Owner, claims)
                .Should().BeFalse();
        }

        [Test, MoqAutoData]
        public void And_User_Is_In_Role_Then_Returns_True(
            string employerAccountId,
            UserClaimsService userClaimsService)
        {
            var claims = BuildClaims(employerAccountId, EmployerUserRole.Owner);

            userClaimsService.UserIsInRole(employerAccountId, EmployerUserRole.Owner, claims)
                .Should().BeTrue();
        }

        [Test, MoqAutoData]
        public void And_Claim_Not_Found_Then_Returns_False(
            string employerAccountId,
            UserClaimsService userClaimsService)
        {
            var claims = new List<Claim>();

            userClaimsService.UserIsInRole(employerAccountId, EmployerUserRole.Owner, claims)
                .Should().BeFalse();
        }

        [Test, MoqAutoData]
        public void And_User_Not_Found_Then_Returns_False(
            string employerAccountId,
            string differentAccountId,
            UserClaimsService userClaimsService)
        {
            var claims = BuildClaims(differentAccountId, EmployerUserRole.Owner);

            userClaimsService.UserIsInRole(employerAccountId, EmployerUserRole.Owner, claims)
                .Should().BeFalse();
        }

        private IEnumerable<Claim> BuildClaims(string employerAccountId, EmployerUserRole userRole)
        {
            var claims = new List<Claim>
            {
                new Claim(EmployerClaims.AccountsClaimsTypeIdentifier,
                    JsonConvert.SerializeObject(
                        new Dictionary<string, EmployerIdentifier>
                        {
                            {
                                employerAccountId, new EmployerIdentifier
                                {
                                    AccountId = employerAccountId,
                                    EmployerName = "Tests That Pass",
                                    Role = userRole.ToString()
                                }
                            }
                        }
                    ))
            };

            return claims;
        }
    }
}