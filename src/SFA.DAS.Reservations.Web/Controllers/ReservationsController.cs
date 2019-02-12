using System;
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

        [Route("apprenticeship-training")]
        public IActionResult ApprenticeshipTraining()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create()
        {
            var accountId = RouteData.Values["employerAccountId"].ToString();
            var command = new CreateReservationCommand
            {
                AccountId = accountId,
                StartDate = DateTime.Today
            };

            await _mediator.Send(command);
            
            return RedirectToAction(nameof(Confirmation), new {employerAccountId = accountId});
        }

        // GET
        [Route("confirmation")]
        public IActionResult Confirmation()
        {
            return View();
        }
    }
}