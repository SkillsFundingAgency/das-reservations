using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SFA.DAS.Reservations.Domain.Authentication;
using SFA.DAS.Reservations.Web.Infrastructure;

namespace SFA.DAS.Reservations.Web.AcceptanceTests.Infrastructure
{
    public class TestAuthenticationHandler : AuthenticationHandler<TestAuthenticationSchemeOptions>
    {
        private readonly string _employerAccountId;

        public TestAuthenticationHandler(IOptionsMonitor<TestAuthenticationSchemeOptions> options, 
            ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
            _employerAccountId = options.Get("IntegrationTest").EmployerAccountId;
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var employerIdentifier  = new EmployerIdentifier
            {
                Role = "Owner",
                EmployerName = "Test",
                AccountId = _employerAccountId
            };
            var dictionary = new Dictionary<string, EmployerIdentifier>{{_employerAccountId,employerIdentifier}};
            
            var claims = new[]
            {
                new Claim(EmployerClaims.IdamsUserIdClaimTypeIdentifier, Guid.NewGuid().ToString()),
                new Claim(EmployerClaims.AccountsClaimsTypeIdentifier, JsonConvert.SerializeObject(dictionary)), 
            };
            var identity = new ClaimsIdentity(claims, "IntegrationTest");
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, "IntegrationTest");

            var result = AuthenticateResult.Success(ticket);

            return Task.FromResult(result);
        }
    }

    public class TestAuthenticationSchemeOptions : AuthenticationSchemeOptions
    {
        public string EmployerAccountId { get; set; }
        
    }
}