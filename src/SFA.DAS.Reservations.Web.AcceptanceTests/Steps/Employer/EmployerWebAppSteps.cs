using SFA.DAS.Reservations.Web.AcceptanceTests.Infrastructure;
using TechTalk.SpecFlow;

namespace SFA.DAS.Reservations.Web.AcceptanceTests.Steps.Employer
{
    [Binding]
    public class EmployerWebAppSteps : StepsBase
    {
        public EmployerWebAppSteps(
            EmployerTestServiceProvider serviceProvider, 
            TestData testData) 
            : base(serviceProvider, testData)
        {
        }

        [Given(@"an employer web app is running")]
        public void GivenAnEmployerWebAppIsRunning()
        {
            ArrangeApiClient();
            ArrangeReservationOuterService();
        }
    }
}