using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.WsFederation;
using Microsoft.AspNetCore.Mvc;

namespace SFA.DAS.Reservations.Web.Controllers
{
    public class HomeController : Controller
    {
        [Route("{ukprn}/signout",Name = "provider-signout")]
        [Route("accounts/{employerAccountId}/signout", Name="employer-signout")]
        public IActionResult SignOut(string ukprn="", string employerAccountId="")
        {
            return SignOut(
                new Microsoft.AspNetCore.Authentication.AuthenticationProperties
                {
                    RedirectUri = "",
                    AllowRefresh = true
                },
                CookieAuthenticationDefaults.AuthenticationScheme,
                WsFederationDefaults.AuthenticationScheme);
        }
    }
}