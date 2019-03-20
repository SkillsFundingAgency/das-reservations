using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SFA.DAS.Reservations.Application.Reservations.Commands;
using SFA.DAS.Reservations.Application.Reservations.Queries;
using SFA.DAS.Reservations.Application.Reservations.Queries.GetCourses;
using SFA.DAS.Reservations.Application.Reservations.Queries.GetReservation;
using SFA.DAS.Reservations.Domain.Courses;
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

        [Route("{ukPrn}/reservations/{Id}/apprenticeship-training", Name = "provider-apprenticeship-training")]
        [Route("accounts/{employerAccountId}/reservations/apprenticeship-training", Name = "employer-apprenticeship-training")]
        public async Task<IActionResult> ApprenticeshipTraining(ReservationsRouteModel routeModel)
        {
            // todo: get existing training details if exist, get from mediator call
            var viewModel = await BuildApprenticeshipTrainingViewModel(routeModel.Ukprn, routeModel.Id.GetValueOrDefault());

            return View(viewModel);
        }

        [Route("{ukPrn}/reservations/{Id}/apprenticeship-training", Name = "provider-create-apprenticeship-training")]
        [Route("accounts/{employerAccountId}/reservations/apprenticeship-training", Name = "employer-create-apprenticeship-training")]
        [HttpPost]
        public async Task<IActionResult> PostApprenticeshipTraining(ReservationsRouteModel routeModel, ApprenticeshipTrainingFormModel formModel)
        {
            CacheReservationResult result;

            try
            {
                StartDateModel startDateModel = null;
                if (!string.IsNullOrWhiteSpace(formModel.TrainingStartDate))
                    startDateModel = JsonConvert.DeserializeObject<StartDateModel>(formModel.TrainingStartDate);
                Course course = null;
                if (!string.IsNullOrWhiteSpace(formModel.CourseId))
                    course = JsonConvert.DeserializeObject<Course>(formModel.CourseId);

                var existingCommand = await _mediator.Send(new GetCachedReservationQuery {Id = routeModel.Id.GetValueOrDefault()});

                if (existingCommand == null)
                {
                    throw new ArgumentException("Could not find reservation with given ID", nameof(routeModel));
                }

                var command = new CacheCreateReservationCommand
                {
                    AccountId = existingCommand.AccountId,
                    AccountLegalEntityId = existingCommand.AccountLegalEntityId,
                    AccountLegalEntityName = existingCommand.AccountLegalEntityName,
                    StartDate = startDateModel?.StartDate.ToString("yyyy-MM"),
                    StartDateDescription = startDateModel?.ToString(),
                    CourseId = course?.Id,
                    CourseDescription = course == null? "Unknown" : $"{course.Title} - Level: {course.Level}"
                };

                result = await _mediator.Send(command);
            }
            catch (ValidationException e)
            {
                foreach (var member in e.ValidationResult.MemberNames)
                {
                    ModelState.AddModelError(member.Split('|')[0], member.Split('|')[1]);
                }

                var model = await BuildApprenticeshipTrainingViewModel(routeModel.Ukprn, routeModel.Id.GetValueOrDefault());
                return View("ApprenticeshipTraining", model);
            }

            routeModel.Id = result.Id;
            var routeName = routeModel.Ukprn == null ? 
                "employer-review" : 
                "provider-review";

            return RedirectToRoute(routeName, routeModel);
        }

        [Route("{ukPrn}/reservations/{id}/review", Name = "provider-review")]
        [Route("accounts/{employerAccountId}/reservations/{id}/review", Name = "employer-review")]
        public async Task<IActionResult> Review(ReservationsRouteModel routeModel)
        {
            GetCachedReservationResult cachedReservation;

            try
            {
                var query = new GetCachedReservationQuery
                {
                    Id = routeModel.Id.GetValueOrDefault()
                };

                cachedReservation = await _mediator.Send(query);
            }
            catch (ValidationException e)
            {
                foreach (var member in e.ValidationResult.MemberNames)
                {
                    ModelState.AddModelError(member.Split('|')[0], member.Split('|')[1]);
                }

                return View("Error");//todo: setup view correctly.
            }
            
            var routeName = routeModel.Ukprn == null ? 
                "employer-create-reservation" : 
                "provider-create-reservation";

            var viewModel = new ReviewViewModel
            {
                RouteName = routeName,
                RouteModel = routeModel,
                StartDateDescription = cachedReservation.StartDateDescription,
                CourseDescription = cachedReservation.CourseDescription,
                AccountLegalEntityName = cachedReservation.AccountLegalEntityName
            };
            return View(viewModel);
        }

        [Route("{ukPrn}/reservations/{id}/create", Name = "provider-create-reservation")]
        [Route("accounts/{employerAccountId}/reservations/{id}/create", Name = "employer-create-reservation")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ReservationsRouteModel routeModel)
        {
            GetCachedReservationResult cachedReservationResult;

            try
            {
                var query = new GetCachedReservationQuery
                {
                    Id = routeModel.Id.GetValueOrDefault()
                };

                cachedReservationResult = await _mediator.Send(query);
            }
            catch (ValidationException e)
            {
                foreach (var member in e.ValidationResult.MemberNames)
                {
                    ModelState.AddModelError(member.Split('|')[0], member.Split('|')[1]);
                }

                return View("Error");//todo: setup view correctly.
            }

            try
            {
                var command = new CreateReservationCommand
                {
                    AccountId = cachedReservationResult.AccountId,
                    StartDate = cachedReservationResult.StartDate,
                    Id = cachedReservationResult.Id,
                    CourseId = cachedReservationResult.CourseId
                };

                await _mediator.Send(command);

                await _mediator.Send(new DeleteCachedReservationCommand {Id = cachedReservationResult.Id});
            }
            catch (ValidationException e)
            {
                foreach (var member in e.ValidationResult.MemberNames)
                {
                    ModelState.AddModelError(member.Split('|')[0], member.Split('|')[1]);
                }

                var model = await BuildApprenticeshipTrainingViewModel(routeModel.Ukprn, routeModel.Id.GetValueOrDefault());
                return View("ApprenticeshipTraining", model);
            }

            return RedirectToRoute(routeModel.Ukprn.HasValue ? "provider-reservation-created" : "employer-reservation-created", routeModel);
        }

        // GET

        [Route("{ukPrn}/reservations/{id}/create", Name = "provider-reservation-created")]
        [Route("accounts/{employerAccountId}/reservations/{id}/create", Name = "employer-reservation-created")]
        public async Task<IActionResult> Confirmation(ReservationsRouteModel routeModel)
        {
            if (!routeModel.Id.HasValue)
            {
                throw new ArgumentException("Reservation ID must be in URL.", nameof(routeModel.Id));
            }

            var query = new GetReservationQuery
            {
                Id = routeModel.Id.Value 
            };
            var queryResult = await _mediator.Send(query);

            var model = new ConfirmationViewModel
            {
                ReservationId = queryResult.ReservationId,
                StartDate = queryResult.StartDate,
                ExpiryDate = queryResult.ExpiryDate,
                Course = queryResult.Course
            };
            return View(model);
        }

        private async Task<ApprenticeshipTrainingViewModel> BuildApprenticeshipTrainingViewModel(uint? ukPrn, Guid reservationId)
        {
            var dates = await _startDateService.GetStartDates();

            var coursesResult = await _mediator.Send(new GetCoursesQuery());

            return new ApprenticeshipTrainingViewModel
            {
                ReservationId = reservationId,
                RouteName = ukPrn == null ? "employer-create-apprenticeship-training" : "provider-create-apprenticeship-training",
                PossibleStartDates = dates.Select(startDateModel => new StartDateViewModel
                {
                    Id = $"{startDateModel.StartDate:yyyy-MM}",
                    Value = JsonConvert.SerializeObject(startDateModel),
                    Label = $"{startDateModel.StartDate:MMMM yyyy}"
                }).OrderBy(model => model.Value),
                Courses = coursesResult.Courses
            };
        }
    }
}