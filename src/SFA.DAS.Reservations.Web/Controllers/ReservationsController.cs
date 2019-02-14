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
    //[Authorize(Policy = nameof(PolicyNames.HasEmployerAccount))]//todo: separate story to get both policies working (poss. as a single policy)
    [Authorize(Policy = nameof(PolicyNames.HasProviderAccount))]
    //[Route("accounts/{employerAccountId}/reservations")] //todo: why defaults to this route, not using provider?
    [Route("{ukprn:int}/accounts/{employerAccountId}/reservations", Name = "provider-reservations")]
    public class ReservationsController : Controller
    {
        private readonly IMediator _mediator;

        public ReservationsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [Route("apprenticeship-training")]
        public IActionResult ApprenticeshipTraining(ReservationsRouteModel routeModel)
        {
            return View();
        }

        [Route("apprenticeship-training")]
        [HttpPost]
        public async Task<IActionResult> PostApprenticeshipTraining(ReservationsRouteModel routeModel)//todo: change model to be args from form
        {
            await Task.CompletedTask;
            return RedirectToAction(nameof(Confirmation), routeModel);
        }

        [Route("create")]
        [HttpPost]
        public async Task<IActionResult> Create(ReservationsRouteModel routeModel)
        {
            var accountId = RouteData.Values["employerAccountId"].ToString();
            var command = new CreateReservationCommand
            {
                AccountId = accountId,
                StartDate = DateTime.Today
            };

            await _mediator.Send(command);
            
            return RedirectToAction(nameof(Confirmation), routeModel);
        }

        // GET
        [Route("review")]
        public IActionResult Review(ReservationsRouteModel routeModel)
        {
            return null;
        }

        // GET
        [Route("confirmation")]
        public IActionResult Confirmation(ReservationsRouteModel routeModel)
        {
            return View();
        }
    }
}