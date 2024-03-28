using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using SFA.DAS.Reservations.Domain.Employers;
using SFA.DAS.Reservations.Web.AcceptanceTests.Infrastructure;

namespace SFA.DAS.Reservations.Web.AcceptanceTests.Tests;

[TestFixture]
public class WhenViewingTheReservationServiceAsNonLevyEmployer
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
                AccountId = TestDataValues.NonLevyAccountId,
                AccountLegalEntityId = TestDataValues.NonLevyAccountLegalEntityId,
                AccountLegalEntityPublicHashedId = TestDataValues.NonLevyHashedAccountLegalEntityId,
                AccountLegalEntityName = "Test Legal Entity",
                IsLevy = false,
                LegalEntityId = 1
            }
        };
        _factory = new TestWebApplicationFactory("employer");
        _client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(collection =>
                collection.ConfigureTestServiceCollection(
                    _factory.ConfigurationRoot, testData));
            builder.ConfigureTestServices(services =>
            {
                services.AddAuthentication("IntegrationTest")
                    .AddScheme<TestAuthenticationSchemeOptions, TestAuthenticationHandler>(
                        "IntegrationTest", options => { options.EmployerAccountId =  TestDataValues.NonLevyHashedAccountId;});
            });
        }).CreateClient();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("IntegrationTest");
    }

    [Test]
    public async Task Then_I_Am_Able_To_View_The_Reservation_Start_Page_As_A_Non_Levy_Employer()
    {
        //Act
        var result = await _client.GetAsync($"/accounts/{TestDataValues.NonLevyHashedAccountId}/reservations");
        
        //Assert
        Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);

        var expectedUrl = HttpUtility.UrlDecode($"/accounts/{TestDataValues.NonLevyHashedAccountId}/reservations");
        var actualUrl = HttpUtility.UrlDecode(result.RequestMessage?.RequestUri?.ToString());

        Assert.IsTrue(actualUrl?.Contains(expectedUrl));
    }
}