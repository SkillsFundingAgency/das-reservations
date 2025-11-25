using SFA.DAS.Reservations.Web.AcceptanceTests.Infrastructure;
using Reqnroll;

namespace SFA.DAS.Reservations.Web.AcceptanceTests.Steps.Provider
{
    [Binding]
    public class ProviderWebAppSteps : StepsBase
    {
        public ProviderWebAppSteps(
            ProviderTestServiceProvider serviceProvider, 
            TestData testData) 
            : base(serviceProvider, testData)
        {
        }

        [Given(@"a provider web app is running")]
        public void GivenAProviderWebAppIsRunning()
        {
            SetupProviderTestData();
            ArrangeApiClient();
        }
    }
}