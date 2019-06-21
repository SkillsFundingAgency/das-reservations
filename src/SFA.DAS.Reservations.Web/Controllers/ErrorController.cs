using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Reservations.Web.Infrastructure;

namespace SFA.DAS.Reservations.Web.Controllers
{
    [Route("error")]
    public class ErrorController : Controller
    {

        [Route("403", Name = RouteNames.Error403)]
        public IActionResult AccessDenied()
        {
            return View();
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