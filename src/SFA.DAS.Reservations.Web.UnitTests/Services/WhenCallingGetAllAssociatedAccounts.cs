using System.Collections.Generic;
using System.Security.Claims;
using FluentAssertions;
using Newtonsoft.Json;
using NUnit.Framework;
using SFA.DAS.Reservations.Domain.Authentication;
using SFA.DAS.Reservations.Domain.Employers;
using SFA.DAS.Reservations.Infrastructure.Services;
using SFA.DAS.Reservations.Web.Services;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Reservations.Web.UnitTests.Services
{
    public class WhenCallingGetAllAssociatedAccounts
    {
        [Test, MoqAutoData]
        public void And_User_Has_Account_Claim(
           string employerAccountId,
           UserClaimsService userClaimsService)
        {
            var claims = BuildClaims(employerAccountId, EmployerUserRole.None);

            userClaimsService.GetAllAssociatedAccounts(claims)
                .Should().HaveCount(1);
        }

        [Test, MoqAutoData]
        public void And_User_Has_No_Claims(
          UserClaimsService userClaimsService)
        {
            var claims = new List<Claim>();

            userClaimsService.GetAllAssociatedAccounts(claims)
                .Should().HaveCount(0);
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
