using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication.WsFederation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using SFA.DAS.GovUK.Auth.Models;
using SFA.DAS.GovUK.Auth.Services;
using SFA.DAS.Reservations.Infrastructure.Configuration;
using SFA.DAS.Reservations.Web.Infrastructure;
using SFA.DAS.Reservations.Web.Models;

namespace SFA.DAS.Reservations.Web.Controllers;

public class HomeController : Controller
{
    private readonly IConfiguration _config;
    private readonly IStubAuthenticationService _stubAuthenticationService;
    private readonly ReservationsWebConfiguration _configuration;

    public HomeController(IConfiguration config, IStubAuthenticationService stubAuthenticationService, IOptions<ReservationsWebConfiguration> configuration)
    {
        _config = config;
        _stubAuthenticationService = stubAuthenticationService;
        _configuration = configuration.Value;
    }

    [Route("accounts/signout", Name = RouteNames.EmployerSignOut)]
    [Route("signout", Name = RouteNames.ProviderSignOut)]
    [HttpGet("service/signout")]
    public IActionResult SignOut()
    {
        if (IsThisAnEmployer())
        {
            var schemes = new List<string>
            {
                CookieAuthenticationDefaults.AuthenticationScheme
            };
            _ = bool.TryParse(_config["StubAuth"], out var stubAuth);

            if (!stubAuth)
            {
                schemes.Add(OpenIdConnectDefaults.AuthenticationScheme);
            }

            return SignOut(new AuthenticationProperties
            {
                RedirectUri = "",
                AllowRefresh = true
            }, schemes.ToArray());
        }

        var useAuthScheme = _configuration.UseDfESignIn
            ? OpenIdConnectDefaults.AuthenticationScheme
            : WsFederationDefaults.AuthenticationScheme;

        return SignOut(
            new AuthenticationProperties
            {
                RedirectUri = "",
                AllowRefresh = true
            },
            CookieAuthenticationDefaults.AuthenticationScheme,
            useAuthScheme);
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
        return View("SigninStub", new List<string> { _config["StubId"], _config["StubEmail"] });
    }
    [HttpPost]
    [Route("SignIn-Stub")]
    public async Task<IActionResult> SigninStubPost()
    {
        var model = new StubAuthUserDetails
        {
            Email = _config["StubEmail"],
            Id = _config["StubId"]
        };
        var claims = await _stubAuthenticationService.GetStubSignInClaims(model);

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
        return _config["AuthType"] != null &&
               _config["AuthType"].Equals("employer", StringComparison.CurrentCultureIgnoreCase);
    }
}