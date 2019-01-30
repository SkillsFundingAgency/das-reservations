using Microsoft.AspNetCore.Mvc;

namespace SFA.DAS.Reservations.Web.Controllers
{
    [Route("accounts/{employerAccountId}/reservations")]
    public class ReservationsController : Controller
    {
        // GET
        public IActionResult Welcome()
        {
            return View();
        }
    }
}