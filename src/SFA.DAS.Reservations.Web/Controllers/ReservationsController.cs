using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Reservations.Application.Reservations.Commands;
using SFA.DAS.Reservations.Web.Infrastructure;

namespace SFA.DAS.Reservations.Web.Controllers
{
    [Authorize(Policy = nameof(PolicyNames.HasEmployerAccount))]
    [Route("accounts/{employerAccountId}/reservations")]
    public class ReservationsController : Controller
    {
        private readonly IMediator _mediator;

        public ReservationsController(IMediator mediator)
        {
            _mediator = mediator;
        }
        // GET
        public IActionResult Welcome()
        {
            return View();
        }

        [HttpPost]
        [Route("create")]
        public IActionResult PostCreate()
        {
            _mediator.Send(new CreateReservationCommand());
            return null;
        }
    }
}