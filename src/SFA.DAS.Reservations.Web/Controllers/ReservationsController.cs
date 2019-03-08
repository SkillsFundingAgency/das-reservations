using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Reservations.Application.Reservations.Commands;
using SFA.DAS.Reservations.Application.Reservations.Queries;
using SFA.DAS.Reservations.Application.Reservations.Queries.GetCourses;
using SFA.DAS.Reservations.Application.Reservations.Queries.GetReservation;
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

        [Route("apprenticeship-training")]
        [Route("{ukPrn}/accounts/{employerAccountId}/reservations/apprenticeship-training", Name = "provider-apprenticeship-training")]
        [Route("accounts/{employerAccountId}/reservations/apprenticeship-training", Name = "employer-apprenticeship-training")]
        public async Task<IActionResult> ApprenticeshipTraining(string employerAccountId, int? ukPrn)
        {
            
            
            var viewModel = await BuildApprenticeshipTrainingViewModel(ukPrn);

            return View(viewModel);
        }

        [Route("apprenticeship-training")]
        [HttpPost]
        public async Task<IActionResult> PostApprenticeshipTraining(ReservationsRouteModel routeModel)//todo: change model to be args from form
        {
            await Task.CompletedTask;//todo: save form data to cache
            return RedirectToAction(nameof(Review), routeModel);//this also defaults to the employer route which then throws error for provider as no ukprn in the route.
        }


        [Route("review")]
        public IActionResult Review(ReservationsRouteModel routeModel)
        {
            return View(routeModel);
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
                return View("ApprenticeshipTraining", model);
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

        private async Task<ApprenticeshipTrainingViewModel> BuildApprenticeshipTrainingViewModel(int? ukPrn)
        {
            var dates = await _startDateService.GetStartDates();

            var coursesResult = await _mediator.Send(new GetCoursesQuery());

            return new ApprenticeshipTrainingViewModel
            {
                RouteName = ukPrn == null ? "employer-create-reservation" : "provider-create-reservation",
                PossibleStartDates = dates.Select(date => new StartDateViewModel
                {
                    Value = $"{date:yyyy-MM}",
                    Label = $"{date:MMMM yyyy}"
                }).OrderBy(model => model.Value),
                Courses = coursesResult.Courses
            };
        }
    }
}