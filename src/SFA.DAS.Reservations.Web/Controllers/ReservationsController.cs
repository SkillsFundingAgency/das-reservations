using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SFA.DAS.Reservations.Application.Reservations.Commands;
using SFA.DAS.Reservations.Application.Reservations.Commands.CacheReservationCourse;
using SFA.DAS.Reservations.Application.Reservations.Commands.CacheReservationStartDate;
using SFA.DAS.Reservations.Application.Reservations.Queries.GetCachedReservation;
using SFA.DAS.Reservations.Application.Reservations.Queries.GetCourses;
using SFA.DAS.Reservations.Application.Reservations.Queries.GetReservation;
using SFA.DAS.Reservations.Domain.Courses;
using SFA.DAS.Reservations.Infrastructure.Configuration.Configuration;
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
        private readonly ReservationsWebConfiguration _configuration;

        public ReservationsController(IMediator mediator, IStartDateService startDateService, IOptions<ReservationsWebConfiguration> configuration)
        {
            _mediator = mediator;
            _startDateService = startDateService;
            _configuration = configuration.Value;
        }

        [Route("{ukPrn}/reservations/{id}/apprenticeship-training", Name = RouteNames.ProviderApprenticeshipTraining)]
        [Route("accounts/{employerAccountId}/reservations/{id}/apprenticeship-training", Name = RouteNames.EmployerApprenticeshipTraining)]
        public async Task<IActionResult> ApprenticeshipTraining(ReservationsRouteModel routeModel)
        {
            GetCachedReservationResult cachedReservation = null;

            if (routeModel.Id.HasValue)
            {
                cachedReservation = await _mediator.Send(new GetCachedReservationQuery {Id = routeModel.Id.GetValueOrDefault()});
                //todo: error handling if fails validation e.g. id not found
            }
            
            var viewModel = await BuildApprenticeshipTrainingViewModel(routeModel.UkPrn, cachedReservation?.CourseId, cachedReservation?.StartDate);

            return View(viewModel);
        }

        [Route("{ukPrn}/reservations/{id}/apprenticeship-training", Name = RouteNames.ProviderCreateApprenticeshipTraining)]
        [Route("accounts/{employerAccountId}/reservations/{id}/apprenticeship-training", Name = RouteNames.EmployerCreateApprenticeshipTraining)]
        [HttpPost]
        public async Task<IActionResult> PostApprenticeshipTraining(ReservationsRouteModel routeModel, ApprenticeshipTrainingFormModel formModel)
        {
            StartDateModel startDateModel = null;
            if (!string.IsNullOrWhiteSpace(formModel.TrainingStartDate))
                startDateModel = JsonConvert.DeserializeObject<StartDateModel>(formModel.TrainingStartDate);

            Course course = null;

            if (!string.IsNullOrEmpty(formModel.SelectedCourseId))
            {
                var getCoursesResult = await _mediator.Send(new GetCoursesQuery());

                var selectedCourse =
                    getCoursesResult.Courses.SingleOrDefault(c => c.Id.Equals(formModel.SelectedCourseId));

                course = selectedCourse ?? throw new ArgumentException("Selected course does not exist", nameof(formModel.SelectedCourseId));
            }

            try
            {
                var existingCommand = await _mediator.Send(new GetCachedReservationQuery {Id = routeModel.Id.GetValueOrDefault()});

                if (existingCommand == null)
                {
                    throw new ArgumentException("Could not find reservation with given ID", nameof(routeModel));
                }

                var courseCommand = new CacheReservationCourseCommand
                {
                    Id = existingCommand.Id,
                    CourseId = course?.Id
                };

                await _mediator.Send(courseCommand);

                var startDateCommand = new CacheReservationStartDateCommand
                {
                    Id = existingCommand.Id,
                    StartDate = startDateModel?.StartDate.ToString("yyyy-MM"),
                    StartDateDescription = startDateModel?.ToString(),
                };

                await _mediator.Send(startDateCommand);
            }
            catch (ValidationException e)
            {
                foreach (var member in e.ValidationResult.MemberNames)
                {
                    ModelState.AddModelError(member.Split('|')[0], member.Split('|')[1]);
                }

                var model = await BuildApprenticeshipTrainingViewModel(routeModel.UkPrn, course?.Id);
                return View("ApprenticeshipTraining", model);
            }

            routeModel.Id = routeModel.Id.GetValueOrDefault();
            var routeName = routeModel.UkPrn == null ? 
                RouteNames.EmployerReview :
                RouteNames.ProviderReview;

            return RedirectToRoute(routeName, routeModel);
        }

        [Route("{ukPrn}/reservations/{id}/review", Name = RouteNames.ProviderReview)]
        [Route("accounts/{employerAccountId}/reservations/{id}/review", Name = RouteNames.EmployerReview)]
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
           
            var viewModel = new ReviewViewModel(routeModel,cachedReservation.StartDateDescription, cachedReservation.CourseDescription, cachedReservation.AccountLegalEntityName, cachedReservation.AccountLegalEntityPublicHashedId);
            return View(viewModel.ViewName,viewModel);
        }

        [Route("{ukPrn}/reservations/{id}/create", Name = RouteNames.ProviderCreateReservation)]
        [Route("accounts/{employerAccountId}/reservations/{id}/create", Name = RouteNames.EmployerCreateReservation)]
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

                var result = await _mediator.Send(command);
                routeModel.AccountLegalEntityPublicHashedId = result.Reservation.AccountLegalEntityPublicHashedId;
            }
            catch (ValidationException e)
            {
                foreach (var member in e.ValidationResult.MemberNames)
                {
                    ModelState.AddModelError(member.Split('|')[0], member.Split('|')[1]);
                }

                var model = await BuildApprenticeshipTrainingViewModel(routeModel.UkPrn);
                return View("ApprenticeshipTraining", model);
            }
            catch (Exception)
            {
                var model = await BuildApprenticeshipTrainingViewModel(routeModel.UkPrn);
                return View("ApprenticeshipTraining", model);
            }

            return RedirectToRoute(routeModel.UkPrn.HasValue ? RouteNames.ProviderReservationCreated : RouteNames.EmployerReservationCreated, routeModel);
        }

        // GET

        [Route("{ukPrn}/reservations/{id}/create/{accountLegalEntityPublicHashedId}", Name = RouteNames.ProviderReservationCreated)]
        [Route("accounts/{employerAccountId}/reservations/{id}/create", Name = RouteNames.EmployerReservationCreated)]
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
            (
                queryResult.ReservationId,
                queryResult.StartDate,
                queryResult.ExpiryDate,
                queryResult.Course,
                routeModel.AccountLegalEntityPublicHashedId,
				routeModel.UkPrn,
                queryResult.AccountLegalEntityName,
                _configuration.DashboardUrl, 
                _configuration.ApprenticeUrl
            );
            return View(model);
        }

        [HttpPost]
        [Route("{ukPrn}/reservations/{id}/create/{accountLegalEntityPublicHashedId}", Name = RouteNames.ProviderReservationCompleted)]
        public IActionResult Completed(ReservationsRouteModel routeModel, ConfirmationRedirectViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var confirmationModel = new ConfirmationViewModel
                (
                    model.ReservationId,
                    model.StartDate,
                    model.ExpiryDate,
                    new Course(model.CourseId, model.CourseTitle, model.Level),
                    routeModel.AccountLegalEntityPublicHashedId,
                    routeModel.UkPrn,
                    model.AccountLegalEntityName,
                    _configuration.DashboardUrl,
                    _configuration.ApprenticeUrl
                );
                
                return View("Confirmation", confirmationModel);
            }

            if (model.AddApprentice.HasValue && model.AddApprentice.Value)
            {
                return Redirect(model.ApprenticeUrl);
            }

            return Redirect(model.DashboardUrl);
        }

        private async Task<ApprenticeshipTrainingViewModel> BuildApprenticeshipTrainingViewModel(long? ukPrn,
            string courseId = null, string startDate = null)
        {
            var dates = await _startDateService.GetStartDates();

            var coursesResult = await _mediator.Send(new GetCoursesQuery());

            return new ApprenticeshipTrainingViewModel
            {
                RouteName = ukPrn == null ? RouteNames.EmployerCreateApprenticeshipTraining : RouteNames.ProviderCreateApprenticeshipTraining,
                PossibleStartDates = dates.Select(startDateModel => new StartDateViewModel(startDateModel, startDate)).OrderBy(model => model.Value),
                Courses = coursesResult.Courses?.Select(course => new CourseViewModel(course, courseId)),
                CourseId = courseId,
                TrainingStartDate = startDate
            };
        }
    }
}
