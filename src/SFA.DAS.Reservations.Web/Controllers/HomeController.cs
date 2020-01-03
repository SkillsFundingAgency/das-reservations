using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication.WsFederation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Web.Filters;
using SFA.DAS.Reservations.Web.Infrastructure;
using SFA.DAS.Reservations.Web.Models;

namespace SFA.DAS.Reservations.Web.Controllers
{
    public class HomeController : Controller
    {    
        private readonly ILogger<HomeController> _logger;
        private readonly IExternalUrlHelper _urlHelper;

        public HomeController(ILogger<HomeController> logger, IExternalUrlHelper urlHelper)
        {
            _logger = logger;
            _urlHelper = urlHelper;
        }

        [Route("signout",Name = RouteNames.ProviderSignOut)]
        public IActionResult SignOut()
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

        [Route("accounts/signout", Name = RouteNames.EmployerSignOut)]
        public IActionResult SignOutEmployer()
        {
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
        
        [Route("{ukPrn}/notAvailable", Name = RouteNames.ProviderFeatureNotAvailable)]
        [Route("accounts/{employerAccountId}/notAvailable", Name = RouteNames.EmployerFeatureNotAvailable)]
        public IActionResult FeatureNotAvailable()
        {
            return View();
        }
    }
}