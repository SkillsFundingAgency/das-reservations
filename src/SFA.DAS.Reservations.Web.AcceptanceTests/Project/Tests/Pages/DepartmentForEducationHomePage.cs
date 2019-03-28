using System;
using OpenQA.Selenium;
using SFA.DAS.Reservations.Web.AcceptanceTests.Project.Framework.Helpers;
using SFA.DAS.Reservations.Web.AcceptanceTests.Project.Tests.TestSupport;

namespace SFA.DAS.Reservations.Web.AcceptanceTests.Project.Tests.Pages
{
    class DepartmentForEducationHomePage : BasePage
    {
        private static String PAGE_TITLE = "";

        public DepartmentForEducationHomePage(IWebDriver webDriver) : base(webDriver)
        {
            SelfVerify();
        }

        protected override bool SelfVerify()
        {
            return PageInteractionHelper.VerifyPageHeading(this.GetPageHeading(), PAGE_TITLE);
        }

        internal bool IsPageHeadingMacthing()
        {
            return PageInteractionHelper.VerifyPageHeading(this.GetPageHeading(), PAGE_TITLE);
        }
    }
}