using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SFA.DAS.GovUK.Auth.Models;
using SFA.DAS.GovUK.Auth.Services;
using SFA.DAS.Reservations.Infrastructure.Configuration;
using SFA.DAS.Reservations.Web.Infrastructure;
using SFA.DAS.Reservations.Web.Models;

namespace SFA.DAS.Reservations.Web.Controllers;

public class HomeController(
    IConfiguration config,
    IStubAuthenticationService stubAuthenticationService,
    IOptions<ReservationsWebConfiguration> configuration,
    ILogger<HomeController> logger)
    : Controller
{
    private readonly ReservationsWebConfiguration _configuration = configuration.Value;

    [Route("accounts/signout", Name = RouteNames.EmployerSignOut)]
    [Route("signout", Name = RouteNames.ProviderSignOut)]
    [HttpGet("service/signout")]
    public async Task SignOut([FromQuery] bool autoSignOut = false)
    {
        logger.LogInformation("TEMP: Signing out");
        if (IsThisAnEmployer())
        {
            logger.LogInformation("TEMP: Signing out Employer");
            var schemes = new List<string>
            {
                CookieAuthenticationDefaults.AuthenticationScheme
            };
            _ = bool.TryParse(config["StubAuth"], out var stubAuth);

            if (!stubAuth)
            {
                schemes.Add(OpenIdConnectDefaults.AuthenticationScheme);
            }

            await Task.WhenAll(schemes.Select(s => HttpContext.SignOutAsync(s)));
        }
        else
        {
            logger.LogInformation("TEMP: Signing out Provider");
            var schemes = new List<string>
            {
                CookieAuthenticationDefaults.AuthenticationScheme,
                OpenIdConnectDefaults.AuthenticationScheme
            };

            await Task.WhenAll(schemes.Select(s => HttpContext.SignOutAsync(s)));
        }

        TempData["AutoSignOut"] = autoSignOut;
    }
    
    [Route("~/p-signed-out", Name = "p-signed-out")]
    [AllowAnonymous]
    public IActionResult ProviderSignedOut()
    {
        logger.LogInformation("TEMP: Provider signed out");
        logger.LogInformation("TEMP: DashboardUrl: {DashboardUrl}", _configuration.DashboardUrl);
        var autoSignOut = TempData["AutoSignOut"] as bool? ?? false;
        var viewModel = new AutoSignOutViewModel(_configuration.DashboardUrl);
        return autoSignOut ? View("AutoSignOut", viewModel) : Redirect(_configuration.DashboardUrl);
    }

    [Route("signoutcleanup")]
    public void SignOutCleanup()
    {
        Response.Cookies.Delete("SFA.DAS.Reservations.Web.Auth");
    }

    [Route("{employerAccountId}/service/password/change", Name = RouteNames.EmployerChangePassword)]
    public IActionResult ChangePassword(ReservationsRouteModel model, bool userCancelled = false)
    {
        return RedirectToRoute(RouteNames.EmployerIndex, model);
    }
    [Route("{employerAccountId}/service/email/change", Name = RouteNames.EmployerChangeEmail)]
    public IActionResult ChangeEmail(ReservationsRouteModel model, bool userCancelled = false)
    {
        return RedirectToRoute(RouteNames.EmployerIndex, null);
    }

    [Route("{ukPrn}/notAvailable", Name = RouteNames.ProviderFeatureNotAvailable)]
    [Route("accounts/{employerAccountId}/notAvailable", Name = RouteNames.EmployerFeatureNotAvailable)]
    public IActionResult FeatureNotAvailable()
    {
        return View();
    }

    [HttpGet]
    [Route("dashboard")]
    public IActionResult Dashboard()
    {
        return RedirectPermanent(_configuration.DashboardUrl);
    }

#if DEBUG
    [HttpGet]
    [Route("SignIn-Stub")]
    public IActionResult SigninStub()
    {
        return View("SigninStub", new List<string> { config["StubId"], config["StubEmail"] });
    }
    [HttpPost]
    [Route("SignIn-Stub")]
    public async Task<IActionResult> SigninStubPost()
    {
        var model = new StubAuthUserDetails
        {
            Email = config["StubEmail"],
            Id = config["StubId"]
        };
        var claims = await stubAuthenticationService.GetStubSignInClaims(model);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claims,
            new AuthenticationProperties());

        return RedirectToRoute("Signed-in-stub");
    }

    [Authorize]
    [HttpGet]
    [Route("signed-in-stub", Name = "Signed-in-stub")]
    public IActionResult SignedInStub()
    {
        return View();
    }
#endif

    private bool IsThisAnEmployer()
    {
        return config["AuthType"] != null &&
               config["AuthType"].Equals("employer", StringComparison.CurrentCultureIgnoreCase);
    }
}