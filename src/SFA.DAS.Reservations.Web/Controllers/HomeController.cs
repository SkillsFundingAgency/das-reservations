using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication.WsFederation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SFA.DAS.Reservations.Web.Infrastructure;
using SFA.DAS.Reservations.Web.Models;

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
        public IActionResult SignOut(string ukprn)
        {
            _logger.LogDebug($"User signed out {ukprn}");
            return SignOut(
                new Microsoft.AspNetCore.Authentication.AuthenticationProperties
                {
                    RedirectUri = "",
                    AllowRefresh = true
                },
                CookieAuthenticationDefaults.AuthenticationScheme,
                WsFederationDefaults.AuthenticationScheme);
        }

        [Route("accounts/{employerAccountId}/signout", Name = RouteNames.EmployerSignOut)]
        public IActionResult SignOutEmployer(string employerAccountId)
        {
            _logger.LogDebug($"User signed out {employerAccountId}");
            return SignOut(
                new Microsoft.AspNetCore.Authentication.AuthenticationProperties
                {
                    RedirectUri = "",
                    AllowRefresh = true
                },
                CookieAuthenticationDefaults.AuthenticationScheme,
                OpenIdConnectDefaults.AuthenticationScheme);
        }


        [Route("signoutcleanup")]
        public void SignOutCleanup()
        {
            Response.Cookies.Delete("SFA.DAS.Reservations.Web.Auth");
        }


        [Route("notAvailable", Name="FeatureNotAvailable")]
        public IActionResult FeatureNotAvailable()
        {
            return View();
        }

        [Route("{employerAccountId}/service/password/change", Name =RouteNames.EmployerChangePassword)]
        public IActionResult ChangePassword(ReservationsRouteModel model,bool userCancelled = false)
        {
            return RedirectToRoute(RouteNames.EmployerIndex,model);
        }
        [Route("{employerAccountId}/service/email/change", Name = RouteNames.EmployerChangeEmail)]
        public IActionResult ChangeEmail(ReservationsRouteModel model, bool userCancelled = false)
        {
            return RedirectToRoute(RouteNames.EmployerIndex,null);
        }
    }
}