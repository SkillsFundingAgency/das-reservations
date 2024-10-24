using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using SFA.DAS.Reservations.Infrastructure.Configuration;
using SFA.DAS.Reservations.Web.Infrastructure;
using SFA.DAS.Reservations.Web.Models;

namespace SFA.DAS.Reservations.Web.Controllers;

[Route("error")]
public class ErrorController : Controller
{
    private readonly IConfiguration _configuration;
    private readonly ReservationsWebConfiguration _reservationsWebConfiguration;

    public ErrorController(IConfiguration configuration, IOptions<ReservationsWebConfiguration> reservationsWebConfiguration)
    {
        _configuration = configuration;
        _reservationsWebConfiguration = reservationsWebConfiguration.Value;
    }

    [Route("403", Name = RouteNames.Error403)]
    public IActionResult AccessDenied()
    {
        return View(new Error403ViewModel(_configuration["ResourceEnvironmentName"])
        {
            UseDfESignIn = _reservationsWebConfiguration.UseDfESignIn,
            DashboardUrl = _reservationsWebConfiguration.DashboardUrl,
        });
    }

    [Route("404", Name = RouteNames.Error404)]
    public IActionResult PageNotFound()
    {
        return View();
    }

    [Route("500", Name = RouteNames.Error500)]
    public IActionResult ApplicationError()
    {
        return View();
    }
}