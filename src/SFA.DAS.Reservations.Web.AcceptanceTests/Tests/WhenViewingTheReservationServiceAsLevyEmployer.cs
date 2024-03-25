using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using SFA.DAS.Reservations.Domain.Employers;
using SFA.DAS.Reservations.Web.AcceptanceTests.Infrastructure;

namespace SFA.DAS.Reservations.Web.AcceptanceTests.Tests;

[TestFixture]
public class WhenViewingTheReservationServiceAsLevyEmployer
{
    private TestWebApplicationFactory _factory;
    private HttpClient _client;

    [OneTimeSetUp]
    public void GivenIAmAEmployerUsingTheReservationService()
    {
        var testData = new TestData
        {
            AccountLegalEntity = new AccountLegalEntity
            {
                AccountId = TestDataValues.LevyAccountId,
                AccountLegalEntityId = TestDataValues.LevyAccountLegalEntityId,
                AccountLegalEntityPublicHashedId = TestDataValues.LevyHashedAccountLegalEntityId,
                AccountLegalEntityName = "Test Legal Entity",
                IsLevy = true,
                LegalEntityId = 1
            }
        };

        _factory = new TestWebApplicationFactory("employer");
        _client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(collection => collection.ConfigureTestServiceCollection(_factory.ConfigurationRoot, testData));
            
            builder.ConfigureTestServices(services =>
            {
                services.AddAuthentication( "IntegrationTest")
                    .AddScheme<TestAuthenticationSchemeOptions, TestAuthenticationHandler>(
                        "IntegrationTest", options => { options.EmployerAccountId =  TestDataValues.LevyHashedAccountId;});
            });
        }).CreateClient();
            
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("IntegrationTest");
    }

    [Test]
    public async Task Then_I_Am_Redirected_To_The_Service_Not_Allowed_As_A_Levy_Employer()
    {
        //Act
        var result = await _client.GetAsync($"/accounts/{TestDataValues.LevyHashedAccountId}/reservations");
            
        //Assert
        Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
        Assert.IsTrue(result.RequestMessage.RequestUri.ToString().Contains($"/error/403"));
    }
}