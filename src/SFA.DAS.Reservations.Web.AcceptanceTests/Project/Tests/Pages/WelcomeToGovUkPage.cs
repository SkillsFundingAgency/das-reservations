using System;
using OpenQA.Selenium;
using SFA.DAS.Reservations.Web.AcceptanceTests.Project.Framework.Helpers;
using SFA.DAS.Reservations.Web.AcceptanceTests.Project.Tests.TestSupport;

namespace SFA.DAS.Reservations.Web.AcceptanceTests.Project.Tests.Pages
{
    public class WelcomeToGovUkPage : BasePage
    {
        private static String PAGE_TITLE = "Welcome to GOV.UK";

        public WelcomeToGovUkPage(IWebDriver webDriver) : base(webDriver)
        {
            SelfVerify();
        }

        protected override bool SelfVerify()
        {
            return PageInteractionHelper.VerifyPageHeading(this.GetPageHeading(), PAGE_TITLE);
        }

        private By searchField = By.Name("q");
        private By searchButton = By.CssSelector(".gem-c-search__submit");

        internal SearchResultsPage EnterSearchTextAndSubmit(String searchText)
        {
            FormCompletionHelper.EnterText(searchField, searchText);
            FormCompletionHelper.ClickElement(searchButton);
            return new SearchResultsPage(webDriver);
        }
    }
}