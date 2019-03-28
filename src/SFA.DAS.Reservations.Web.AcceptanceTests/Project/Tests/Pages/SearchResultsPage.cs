using System;
using OpenQA.Selenium;
using SFA.DAS.Reservations.Web.AcceptanceTests.Project.Framework.Helpers;
using SFA.DAS.Reservations.Web.AcceptanceTests.Project.Tests.TestSupport;

namespace SFA.DAS.Reservations.Web.AcceptanceTests.Project.Tests.Pages
{
    public class SearchResultsPage : BasePage
    {
        private static String PAGE_TITLE = "";

        public SearchResultsPage(IWebDriver webDriver) : base(webDriver)
        {
            SelfVerify();
        }

        protected override bool SelfVerify()
        {
            return PageInteractionHelper.VerifyPageHeading(this.GetPageHeading(), PAGE_TITLE);
        }

        private By dfeLink = By.LinkText("Department for Education");

        internal DepartmentForEducationHomePage ClickDfeLink()
        {
            FormCompletionHelper.ClickElement(dfeLink);
            return new DepartmentForEducationHomePage(webDriver);
        }
    }
}