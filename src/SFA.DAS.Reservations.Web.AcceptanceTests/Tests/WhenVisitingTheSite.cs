using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.EnvironmentVariables;
using Microsoft.Extensions.Configuration.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using NUnit.Framework;
using SFA.DAS.Reservations.Domain.Authentication;
using SFA.DAS.Reservations.Infrastructure.Configuration;
using SFA.DAS.Reservations.Web.AcceptanceTests.Infrastructure;
using SFA.DAS.Reservations.Web.Infrastructure;

namespace SFA.DAS.Reservations.Web.AcceptanceTests.Tests
{
    public class TestWebApplicationFactory : WebApplicationFactory<Startup>
    {
        public IConfigurationRoot ConfigurationRoot { get ; set ; }


        public TestWebApplicationFactory (string authType)
        {
            ConfigurationRoot =  TestServiceProvider.GenerateConfiguration(authType, true);
        }

        protected override IWebHostBuilder CreateWebHostBuilder()
        {
            return base.CreateWebHostBuilder().ConfigureAppConfiguration(c =>
            {
                c.AddConfiguration(ConfigurationRoot);
                c.AddEnvironmentVariables("ASPNETCORE");
            });
        }
    }
    public class TestAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public TestAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, 
            ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var employerIdentifier  = new EmployerIdentifier
            {
                Role = "Owner",
                EmployerName = "Test",
                AccountId = TestDataValues.NonLevyHashedAccountId
            };
            var dictionary = new Dictionary<string, EmployerIdentifier>{{TestDataValues.NonLevyHashedAccountId,employerIdentifier}};
            
            var claims = new[]
            {
                new Claim(EmployerClaims.IdamsUserIdClaimTypeIdentifier, Guid.NewGuid().ToString()),
                new Claim(EmployerClaims.AccountsClaimsTypeIdentifier, JsonConvert.SerializeObject(dictionary)), 
            };
            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, "Test");

            var result = AuthenticateResult.Success(ticket);

            return Task.FromResult(result);
        }
    }
    
    [TestFixture]
    public class WhenViewingTheReservationService
    {
        private TestWebApplicationFactory _factory;
        private HttpClient _client;

        [OneTimeSetUp]
        public void GivenIAmAEmployerUsingTheReservationService()
        {
            _factory = new TestWebApplicationFactory("employer");
            _client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(collection =>
                    collection.ConfigureTestServiceCollection(
                        _factory.ConfigurationRoot));
                builder.ConfigureTestServices(services =>
                {
                    services.AddAuthentication("Test")
                        .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>(
                            "Test", options => { });
                });
            }).CreateClient();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");
            
        }


        [Test]
        public async Task Then_I_Am_Able_To_View_The_Reservation_Start_Page_As_A_Non_Levy_Employer()
        {
            //Act
            var result = await _client.GetAsync($"/accounts/{TestDataValues.NonLevyHashedAccountId}/reservations");
            
            //Assert
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            Assert.IsTrue(result.RequestMessage.RequestUri.ToString().Contains($"/accounts/{TestDataValues.NonLevyHashedAccountId}/reservations/start"));
        }

//        [Test]
//        public async Task Then_I_Am_Redirected_To_The_Service_not_Allowed_As_A_Levy_Employer()
//        {
//            //Act
//            var result = await _client.GetAsync($"/accounts/{TestDataValues.LevyHashedAccountId}/reservations");
//            
//            //Assert
//            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
//            Assert.IsTrue(result.RequestMessage.RequestUri.ToString().Contains($"/error/403"));
//        }

    }
}