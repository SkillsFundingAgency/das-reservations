using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using SFA.DAS.Reservations.Infrastructure.Configuration;
using SFA.DAS.Reservations.Web.Infrastructure;
using SFA.DAS.Reservations.Web.Models;
using SFA.DAS.EmployerUrlHelper;

namespace SFA.DAS.Reservations.Web.Controllers
{
    [Route("error")]
    public class ErrorController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly ReservationsWebConfiguration _reservationsWebConfiguration;
        private readonly ILinkGenerator _linkGenerator;

        public ErrorController(IConfiguration configuration, IOptions<ReservationsWebConfiguration> reservationsWebConfiguration, ILinkGenerator linkGenerator)
        {
            _configuration = configuration;
            _reservationsWebConfiguration = reservationsWebConfiguration.Value;
            _linkGenerator = linkGenerator;
        }

        [Route("403", Name = RouteNames.Error403)]
        public IActionResult AccessDenied()
        {

            return View(new Error403ViewModel(_configuration["ResourceEnvironmentName"])
            {
                UseDfESignIn = _reservationsWebConfiguration.UseDfESignIn,
                DashboardUrl = _reservationsWebConfiguration.DashboardUrl,
                ServiceHomePageUrl = _linkGenerator.AccountsLink("")
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
}