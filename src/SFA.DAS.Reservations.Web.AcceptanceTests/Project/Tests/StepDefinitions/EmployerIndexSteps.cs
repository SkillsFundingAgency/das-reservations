using System;
using System.ComponentModel;
using ESFA.UI.Specflow.Framework.Project.Tests.Pages;
using ESFA.UI.Specflow.Framework.Project.Tests.TestSupport;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using SFA.DAS.Reservations.Web.AcceptanceTests.Project.Tests.Pages;
using TechTalk.SpecFlow;

namespace SFA.DAS.Reservations.Web.AcceptanceTests.Project.Tests.StepDefinitions
{
    [Binding]
    public class EmployerIndexSteps : BaseTest
    {
        [Given(@"The employer is viewing the employer Index page")]
        public void GivenTheEmployerIsViewingTheEmployerIndexPage()
        {
            webDriver = new ChromeDriver {Url = Configurator.GetConfiguratorInstance().GetBaseUrl()};

        }
        
        [When(@"The employer presses start")]
        public void WhenTheEmployerPressesStart()
        {
            var employerIndexPage = new EmployerIndexPage(webDriver);
            employerIndexPage.ClickStartButton();
        }
        
        [When(@"The employer presses the apprenticeship training link")]
        public void WhenTheEmployerPressesTheApprenticeshipTrainingLink()
        {
            var employerIndexPage = new EmployerIndexPage(webDriver);
            employerIndexPage.ClickAnchorLink();
        }
        
        [Then(@"Navigate the employer to the page where they select the legal entity")]
        public void ThenNavigateTheEmployerToThePageWhereTheySelectTheLegalEntity()
        {
            var apprenticeshipTrainingSelectionPage = new ApprenticeshipTrainingSelectionPage(webDriver);   
        }

        [Then(@"The employer is redirected to Find Apprenticeship Training via a new tab")]
        public void ThenTheEmployerIsRedirectedToFindApprenticeshipTrainingViaANewTab()
        {
            var findApprenticeshipTrainingPage = new FindApprenticeshipTrainingPage(webDriver);
        }

    }
}
