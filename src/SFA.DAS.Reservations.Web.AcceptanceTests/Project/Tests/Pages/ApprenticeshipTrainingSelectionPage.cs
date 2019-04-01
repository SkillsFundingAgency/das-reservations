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
    class ApprenticeshipTrainingSelectionPage : BasePage
    {
        public static string PAGE_TITLE = "Apprenticeship training";

        public ApprenticeshipTrainingSelectionPage(IWebDriver webDriver) : base(webDriver)
        {
            SelfVerify();
        }

        protected override bool SelfVerify()
        {
            return PageInteractionHelper.VerifyPageHeading(this.GetPageHeading(), PAGE_TITLE);
        }
    }
}
