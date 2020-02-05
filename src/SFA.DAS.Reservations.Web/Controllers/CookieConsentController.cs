using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace SFA.DAS.Reservations.Web.Controllers
{
    public class CookieConsentController : ReservationsBaseController
    {
        public CookieConsentController(IMediator mediator) : base(mediator)
        {
        }

        [HttpGet]
        [Route("cookieConsent")]        
        public ActionResult Settings(bool saved = false)
        {
            return View(new { Saved = saved });
        }

        [HttpPost]
       
        [Route("cookieConsent")]        
        public ActionResult Settings(bool analyticsConsent, bool marketingConsent)
        {
            Response.Cookies.Append("DASSeenCookieMessage", "true");
            Response.Cookies.Append("AnalyticsConsent", analyticsConsent ? "true" : "false");
            Response.Cookies.Append("MarketingConsent", marketingConsent ? "true" : "false");

            return RedirectToAction("Settings", new { saved = true });
        }

        [HttpGet]
        [Route("cookieConsent/details")]
        public ActionResult Details()
        {
            return View();
        }
    }
}
