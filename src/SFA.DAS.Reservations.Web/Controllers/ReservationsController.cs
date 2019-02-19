using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Reservations.Application.Reservations.Commands;
using SFA.DAS.Reservations.Web.Infrastructure;
using SFA.DAS.Reservations.Web.Models;

namespace SFA.DAS.Reservations.Web.Controllers
{
    [Authorize(Policy = nameof(PolicyNames.HasProviderOrEmployerAccount))]
    public class ReservationsController : Controller
    {
        private readonly IMediator _mediator;

        public ReservationsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [Route("{ukPrn}/accounts/{employerAccountId}/reservations/create", Name = "provider-create-reservation")]
        [Route("accounts/{employerAccountId}/reservations/create", Name = "employer-create-reservation")]
        [HttpPost]
        public async Task<IActionResult> Create(string employerAccountId, int? ukPrn)
        {
            var command = new CreateReservationCommand
            {
                AccountId = employerAccountId,
                StartDate = DateTime.Today
            };

            await _mediator.Send(command);

            return RedirectToRoute(ukPrn.HasValue ? "provider-reservation-created" : "employer-reservation-created");
        }

        // GET
        [Route("review")]
        public IActionResult Review(ReservationsRouteModel routeModel)
        {
            return null;
        }

        // GET
        [Route("{ukPrn}/accounts/{employerAccountId}/reservations/create", Name = "provider-reservation-created")]
        [Route("accounts/{employerAccountId}/reservations/create", Name = "employer-reservation-created")]
        public IActionResult Confirmation()
        {
            return View();
        }
    }
}