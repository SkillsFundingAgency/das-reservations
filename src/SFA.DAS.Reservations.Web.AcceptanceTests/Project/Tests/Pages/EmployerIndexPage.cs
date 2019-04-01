using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ESFA.UI.Specflow.Framework.Project.Framework.Helpers;
using ESFA.UI.Specflow.Framework.Project.Tests.TestSupport;
using OpenQA.Selenium;
using SFA.DAS.Reservations.Web.AcceptanceTests.Project.Tests.Pages;

namespace ESFA.UI.Specflow.Framework.Project.Tests.Pages
{
    public class EmployerIndexPage : BasePage
    {
        private static String PAGE_TITLE = "Get funding for taking on an apprentice";

        public EmployerIndexPage(IWebDriver webDriver) : base(webDriver)
        {
            SelfVerify();
        }

        protected override bool SelfVerify()
        {
            return PageInteractionHelper.VerifyPageHeading(this.GetPageHeading(), PAGE_TITLE);
        }

        private readonly By startButton = By.ClassName("govuk-button govuk-button--start");
        private readonly By anchorLink = By.LinkText("https://findapprenticeshiptraining.apprenticeships.education.gov.uk/");

        internal ApprenticeshipTrainingSelectionPage ClickStartButton()
        {
            FormCompletionHelper.ClickElement(startButton);
            return new ApprenticeshipTrainingSelectionPage(webDriver);
        }

        internal FindApprenticeshipTrainingPage ClickAnchorLink()
        {
            FormCompletionHelper.ClickElement(anchorLink);
            return new FindApprenticeshipTrainingPage(webDriver);
        }
    }
}
