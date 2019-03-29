using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.WsFederation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SFA.DAS.Reservations.Web.Infrastructure;

namespace SFA.DAS.Reservations.Web.Controllers
{
    public class HomeController : Controller
    {    
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        [Route("{ukprn}/signout",Name = RouteNames.ProviderSignOut)]
        [Route("accounts/{employerAccountId}/signout", Name=RouteNames.EmployerSignOut)]
        public IActionResult SignOut(string ukprn="", string employerAccountId="")
        {
            _logger.LogDebug($"User signed out {ukprn}{employerAccountId}");
            return SignOut(
                new Microsoft.AspNetCore.Authentication.AuthenticationProperties
                {
                    RedirectUri = "",
                    AllowRefresh = true
                },
                CookieAuthenticationDefaults.AuthenticationScheme,
                WsFederationDefaults.AuthenticationScheme);
        }

        [Route("notAvailable", Name="FeatureNotAvailable")]
        public IActionResult FeatureNotAvailable()
        {
            return View();
        }
         
    }
}