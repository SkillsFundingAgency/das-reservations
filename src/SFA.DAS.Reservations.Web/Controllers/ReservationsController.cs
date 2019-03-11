using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Reservations.Application.Reservations.Commands;
using SFA.DAS.Reservations.Application.Reservations.Queries;
using SFA.DAS.Reservations.Web.Infrastructure;
using SFA.DAS.Reservations.Web.Models;
using SFA.DAS.Reservations.Web.Services;

namespace SFA.DAS.Reservations.Web.Controllers
{
    [Authorize(Policy = nameof(PolicyNames.HasProviderOrEmployerAccount))]
    public class ReservationsController : Controller
    {
        private readonly IMediator _mediator;
        private readonly IStartDateService _startDateService;

        public ReservationsController(IMediator mediator, IStartDateService startDateService)
        {
            _mediator = mediator;
            _startDateService = startDateService;
        }

        [Route("{ukPrn}/accounts/{employerAccountId}/reservations/apprenticeship-training", Name = "provider-apprenticeship-training")]
        [Route("accounts/{employerAccountId}/reservations/apprenticeship-training", Name = "employer-apprenticeship-training")]
        public async Task<IActionResult> ApprenticeshipTraining(string employerAccountId, int? ukPrn)
        {
            // todo: get existing training details if exist, get from mediator call
            var viewModel = await BuildApprenticeshipTrainingViewModel(ukPrn);

            return View(viewModel);
        }

        [Route("{ukPrn}/accounts/{employerAccountId}/reservations/apprenticeship-training", Name = "provider-create-apprenticeship-training")]
        [Route("accounts/{employerAccountId}/reservations/apprenticeship-training", Name = "employer-create-apprenticeship-training")]
        [HttpPost]
        public async Task<IActionResult> PostApprenticeshipTraining(ReservationsRouteModel routeModel, ApprenticeshipTrainingFormModel formModel)
        {
            CacheReservationResult result;

            try
            {
                var command = new CacheReservationCommand
                {
                    AccountId = routeModel.EmployerAccountId,
                    StartDate = formModel.StartDate
                };

                result = await _mediator.Send(command);
            }
            catch (ValidationException e)
            {
                foreach (var member in e.ValidationResult.MemberNames)
                {
                    ModelState.AddModelError(member.Split('|')[0], member.Split('|')[1]);
                }

                var model = await BuildApprenticeshipTrainingViewModel(routeModel.Ukprn);
                return View("ApprenticeshipTraining", model);//todo: view dependent on ukprn.
            }

            routeModel.Id = result.Id;
            var routeName = routeModel.Ukprn == null ? 
                "employer-review" : 
                "provider-review";

            return RedirectToRoute(routeName, routeModel);
        }

        [Route("{ukPrn}/accounts/{employerAccountId}/reservations/review/{id}", Name = "provider-review")]
        [Route("accounts/{employerAccountId}/reservations/review/{id}", Name = "employer-review")]
        public async Task<IActionResult> Review(ReservationsRouteModel routeModel)
        {
            GetCachedReservationResult result;

            try
            {
                var query = new GetCachedReservationQuery
                {
                    Id = routeModel.Id.GetValueOrDefault()
                };

                result = await _mediator.Send(query);
            }
            catch (ValidationException e)
            {
                foreach (var member in e.ValidationResult.MemberNames)
                {
                    ModelState.AddModelError(member.Split('|')[0], member.Split('|')[1]);
                }

                var model = await BuildApprenticeshipTrainingViewModel(routeModel.Ukprn);
                return View("Error", model);//todo: setup view correctly.
            }

            var viewModel = new ReviewViewModel
            {
                RouteModel = routeModel,
                StartDate = result.StartDate
            };
            return View(viewModel);//todo: update view to hit create end point below
        }

        [Route("{ukPrn}/accounts/{employerAccountId}/reservations/create", Name = "provider-create-reservation")]
        [Route("accounts/{employerAccountId}/reservations/create", Name = "employer-create-reservation")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string employerAccountId, int? ukPrn, string trainingStartDate)
        {
            CreateReservationResult result;

            try
            {
                var command = new CreateReservationCommand
                {
                    AccountId = employerAccountId,
                    StartDate = trainingStartDate
                };

                result = await _mediator.Send(command);
            }
            catch (ValidationException e)
            {
                foreach (var member in e.ValidationResult.MemberNames)
                {
                    ModelState.AddModelError(member.Split('|')[0], member.Split('|')[1]);
                }

                var model = await BuildApprenticeshipTrainingViewModel(ukPrn);
                return View("ApprenticeshipTraining", model);//todo: view dependent on ukprn.
            }

            return RedirectToRoute(ukPrn.HasValue ? "provider-reservation-created" : "employer-reservation-created", new {result.Reservation.Id});
        }

        // GET

        [Route("{ukPrn}/accounts/{employerAccountId}/reservations/create/{id}", Name = "provider-reservation-created")]
        [Route("accounts/{employerAccountId}/reservations/create/{id}", Name = "employer-reservation-created")]
        public async Task<IActionResult> Confirmation(ReservationsRouteModel routeModel)
        {
            var query = new GetReservationQuery
            {
                Id = routeModel.Id.Value 
            };
            var queryResult = await _mediator.Send(query);

            var model = new ConfirmationViewModel
            {
                ReservationId = queryResult.ReservationId,
                StartDate = queryResult.StartDate,
                ExpiryDate = queryResult.ExpiryDate
            };
            return View(model);
        }

        private async Task<ApprenticeshipTrainingViewModel> BuildApprenticeshipTrainingViewModel(long? ukPrn)
        {
            var dates = await _startDateService.GetStartDates();
            return new ApprenticeshipTrainingViewModel
            {
                RouteName = ukPrn == null ? "employer-create-reservation" : "provider-create-reservation",
                PossibleStartDates = dates.Select(date => new StartDateViewModel
                {
                    Value = $"{date:yyyy-MM}",
                    Label = $"{date:MMMM yyyy}"
                }).OrderBy(model => model.Value)
            };
        }
    }
}