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
using SFA.DAS.Reservations.Application.Reservations.Queries.GetCachedReservation;
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
            GetCachedReservationResult cachedReservation = null;

            if (routeModel.Id.HasValue)
            {
                cachedReservation = await _mediator.Send(new GetCachedReservationQuery {Id = routeModel.Id.GetValueOrDefault()});
                //todo: error handling if fails validation e.g. id not found
            }
            
            var viewModel = await BuildApprenticeshipTrainingViewModel(routeModel.Ukprn, cachedReservation?.CourseId, cachedReservation?.StartDate);

            return View(viewModel);
        }

        [Route("{ukPrn}/reservations/{Id}/apprenticeship-training", Name = "provider-create-apprenticeship-training")]
        [Route("accounts/{employerAccountId}/reservations/apprenticeship-training", Name = "employer-create-apprenticeship-training")]
        [HttpPost]
        public async Task<IActionResult> PostApprenticeshipTraining(ReservationsRouteModel routeModel, ApprenticeshipTrainingFormModel formModel)
        {
            CacheReservationResult result;

            StartDateModel startDateModel = null;
            if (!string.IsNullOrWhiteSpace(formModel.TrainingStartDate))
                startDateModel = JsonConvert.DeserializeObject<StartDateModel>(formModel.TrainingStartDate);
            Course course = null;
            if (!string.IsNullOrWhiteSpace(formModel.SelectedCourse))
                course = JsonConvert.DeserializeObject<Course>(formModel.SelectedCourse);

            try
            {
                var existingCommand = await _mediator.Send(new GetCachedReservationQuery {Id = routeModel.Id.GetValueOrDefault()});

                if (existingCommand == null)
                {
                    throw new ArgumentException("Could not find reservation with given ID", nameof(routeModel));
                }

                var command = new CacheCreateReservationCommand
                {
                    Id = existingCommand.Id,
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

                var model = await BuildApprenticeshipTrainingViewModel(routeModel.Ukprn, course?.Id);

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
            
            var confirmRouteName = routeModel.Ukprn == null ? 
                "employer-create-reservation" : 
                "provider-create-reservation";
            var changeRouteName = routeModel.Ukprn == null ? 
                "employer-apprenticeship-training" : 
                "provider-apprenticeship-training";

            var viewModel = new ReviewViewModel
            {
                ConfirmRouteName = confirmRouteName,
                ChangeRouteName = changeRouteName,
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
            try
            {
                var command = new CreateReservationCommand
                {
                    Id = routeModel.Id.GetValueOrDefault()
                };

                await _mediator.Send(command);
            }
            catch (ValidationException e)
            {
                foreach (var member in e.ValidationResult.MemberNames)
                {
                    ModelState.AddModelError(member.Split('|')[0], member.Split('|')[1]);
                }

                var model = await BuildApprenticeshipTrainingViewModel(routeModel.Ukprn);
                return View("ApprenticeshipTraining", model);
            }
            catch (Exception ex)
            {
                var model = await BuildApprenticeshipTrainingViewModel(routeModel.Ukprn);
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
                Course = queryResult.Course,
                AccountLegalEntityName = queryResult.AccountLegalEntityName
            };
            return View(model);
        }

        private async Task<ApprenticeshipTrainingViewModel> BuildApprenticeshipTrainingViewModel(long? ukPrn,
            string courseId = null, string startDate = null)
        {
            var dates = await _startDateService.GetStartDates();

            var coursesResult = await _mediator.Send(new GetCoursesQuery());

            return new ApprenticeshipTrainingViewModel
            {
                RouteName = ukPrn == null
                    ? "employer-create-apprenticeship-training"
                    : "provider-create-apprenticeship-training",
                PossibleStartDates = dates.Select(startDateModel => new StartDateViewModel(startDateModel, startDate))
                    .OrderBy(model => model.Value),
                Courses = coursesResult.Courses?.Select(course => new CourseViewModel(course, courseId)),
                CourseId = courseId,
                TrainingStartDate = startDate
            };
        }
    }
}