using System;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Reservations.Application.Reservations.Commands;
using SFA.DAS.Reservations.Web.Infrastructure;
using SFA.DAS.Reservations.Web.Models;
using SFA.DAS.Reservations.Web.Services;

namespace SFA.DAS.Reservations.Web.Controllers
{
    //[Authorize(Policy = nameof(PolicyNames.HasEmployerAccount))]//todo: separate story to get both policies working (poss. as a single policy)
    [Authorize(Policy = nameof(PolicyNames.HasProviderAccount))]
    //[Route("accounts/{employerAccountId}/reservations")] //todo: why defaults to this route, not using provider?
    [Route("{ukprn:int}/accounts/{employerAccountId}/reservations", Name = "provider-reservations")]
    public class ReservationsController : Controller
    {
        private readonly IMediator _mediator;
        private readonly IStartDateService _startDateService;

        public ReservationsController(IMediator mediator, IStartDateService startDateService)
        {
            _mediator = mediator;
            _startDateService = startDateService;
        }

        [Route("apprenticeship-training")]
        public async Task<IActionResult> ApprenticeshipTraining(ReservationsRouteModel routeModel)
        {
            var dates = await _startDateService.GetStartDates();
            
            var viewModel = new ApprenticeshipTrainingViewModel
            {
                RouteModel = routeModel,
                PossibleStartDates = dates.Select(date => new StartDateViewModel
                {
                    Value = $"{date:yyyy-MM}",
                    Label = $"{date:MMMM yyyy}"
                }).OrderBy(model => model.Value)
            };

            return View(viewModel);
        }

        [Route("apprenticeship-training")]
        [HttpPost]
        public async Task<IActionResult> PostApprenticeshipTraining(ReservationsRouteModel routeModel)//todo: change model to be args from form
        {
            await Task.CompletedTask;//todo: save form data to cache
            return RedirectToAction(nameof(Review), routeModel);//this also defaults to the employer route which then throws error for provider as no ukprn in the route.
        }

        // GET
        [Route("review")]
        public IActionResult Review(ReservationsRouteModel routeModel)
        {
            return View(routeModel);
        }

        // GET
        [Route("confirmation")]
        public IActionResult Confirmation(ReservationsRouteModel routeModel)
        {
            return View(routeModel);
        }

        [Route("create")]
        [HttpPost]
        public async Task<IActionResult> Create(ReservationsRouteModel routeModel)
        {
            var command = new CreateReservationCommand
            {
                AccountId = routeModel.EmployerAccountId,
                StartDate = DateTime.Today
            };

            await _mediator.Send(command);
            
            return RedirectToAction(nameof(Confirmation), routeModel);
        }
    }
}