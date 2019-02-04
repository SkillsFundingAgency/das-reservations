using System.Threading.Tasks;
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
        public async Task<IActionResult> PostCreate()
        {
            var accountId = RouteData.Values["employerAccountId"].ToString(); //todo: get from elsewhere???
            var command = new CreateReservationCommand
            {
                AccountId = accountId
            };

            await _mediator.Send(command);
            
            return null;
        }
    }
}