using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ESFA.UI.Specflow.Framework.Project.Framework.Helpers;
using ESFA.UI.Specflow.Framework.Project.Tests.TestSupport;
using OpenQA.Selenium;

namespace SFA.DAS.Reservations.Web.AcceptanceTests.Project.Tests.Pages
{
    class FindApprenticeshipTrainingPage : BasePage
    {
        public static string PAGE_TITLE = "Find apprenticeship training";

        public FindApprenticeshipTrainingPage(IWebDriver webDriver) : base(webDriver)
        {
            SelfVerify();
        }

        protected override bool SelfVerify()
        {
            var browserTabs = webDriver.WindowHandles;
            webDriver.SwitchTo().Window(browserTabs[1]);
            var isCorrectPage = PageInteractionHelper.VerifyPageHeading(this.GetPageHeading(), PAGE_TITLE);
            webDriver.Close();
            webDriver.SwitchTo().Window(browserTabs[0]);
            return isCorrectPage;
        }
    }
}
