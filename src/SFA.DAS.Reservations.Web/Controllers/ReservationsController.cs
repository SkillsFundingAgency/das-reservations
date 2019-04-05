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
            
            var viewModel = await BuildApprenticeshipTrainingViewModel(routeModel.UkPrn != null, cachedReservation?.CourseId, cachedReservation?.StartDate);

            return View(viewModel);
        }

        [Route("{ukPrn}/reservations/{id}/apprenticeship-training", Name = RouteNames.ProviderCreateApprenticeshipTraining)]
        [Route("accounts/{employerAccountId}/reservations/{id}/apprenticeship-training", Name = RouteNames.EmployerCreateApprenticeshipTraining)]
        [HttpPost]
        public async Task<IActionResult> PostApprenticeshipTraining(ReservationsRouteModel routeModel, ApprenticeshipTrainingFormModel formModel)
        {
            var isProvider = routeModel.UkPrn != null;
            StartDateModel startDateModel = null;

            if (!ModelState.IsValid)
            {
                var model = await BuildApprenticeshipTrainingViewModel(isProvider, formModel.SelectedCourseId);
                return View("ApprenticeshipTraining", model);
            }


            if (!string.IsNullOrWhiteSpace(formModel.StartDate))
                startDateModel = JsonConvert.DeserializeObject<StartDateModel>(formModel.StartDate);

            try
            {
                if (isProvider)
                {
                    var courseCommand = new CacheReservationCourseCommand
                    {
                        Id = routeModel.Id.Value,
                        CourseId = formModel.SelectedCourseId
                    };

                    await _mediator.Send(courseCommand);
                }

                var startDateCommand = new CacheReservationStartDateCommand
                {
                    Id = routeModel.Id.Value,
                    StartDate = startDateModel?.StartDate.ToString("yyyy-MM"),
                    StartDateDescription = startDateModel?.ToString()
                };

                await _mediator.Send(startDateCommand);
            }
            catch (ValidationException e)
            {
                foreach (var member in e.ValidationResult.MemberNames)
                {
                    ModelState.AddModelError(member.Split('|')[0], member.Split('|')[1]);
                }
                
                var model = await BuildApprenticeshipTrainingViewModel(isProvider, formModel.SelectedCourseId);
                return View("ApprenticeshipTraining", model);
            }

            var reviewRouteName = isProvider ? 
                RouteNames.ProviderReview :
                RouteNames.EmployerReview;

            return RedirectToRoute(reviewRouteName, routeModel);
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
           
            var viewModel = new ReviewViewModel(
                routeModel,
                cachedReservation.StartDateDescription, 
                cachedReservation.CourseDescription, 
                cachedReservation.AccountLegalEntityName, 
                cachedReservation.AccountLegalEntityPublicHashedId);
            
            return View(viewModel.ViewName, viewModel);
        }

        [Route("{ukPrn}/reservations/{id}/review", Name = RouteNames.ProviderPostReview)]
        [Route("accounts/{employerAccountId}/reservations/{id}/review", Name = RouteNames.EmployerPostReview)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PostReview(ReservationsRouteModel routeModel)
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

                var model = await BuildApprenticeshipTrainingViewModel(routeModel.UkPrn != null);
                return View("ApprenticeshipTraining", model);
            }
            catch (Exception)
            {
                // todo: log this ex
                var model = await BuildApprenticeshipTrainingViewModel(routeModel.UkPrn != null);
                return View("ApprenticeshipTraining", model);
            }

            return RedirectToRoute(routeModel.UkPrn.HasValue ? RouteNames.ProviderCompleted : RouteNames.EmployerCompleted, routeModel);
        }

        // GET

        [Route("{ukPrn}/reservations/{id}/completed/{accountLegalEntityPublicHashedId}", Name = RouteNames.ProviderCompleted)]
        [Route("accounts/{employerAccountId}/reservations/{id}/completed/{accountLegalEntityPublicHashedId}", Name = RouteNames.EmployerCompleted)]
        public async Task<IActionResult> Completed(ReservationsRouteModel routeModel)
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
            //todo: null check on result

            var model = new CompletedViewModel
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
        [Route("{ukPrn}/reservations/{id}/completed/{accountLegalEntityPublicHashedId}", Name = RouteNames.ProviderPostCompleted)]
        [Route("accounts/{employerAccountId}/reservations/{id}/completed/{accountLegalEntityPublicHashedId}", Name = RouteNames.EmployerPostCompleted)]
        public IActionResult PostCompleted(ReservationsRouteModel routeModel, ConfirmationRedirectViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var confirmationModel = new CompletedViewModel
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
                
                return View("Completed", confirmationModel);
            }

            if (model.AddApprentice.HasValue && model.AddApprentice.Value)
            {
                return Redirect(model.ApprenticeUrl);
            }

            return Redirect(model.DashboardUrl);
        }

        private async Task<ApprenticeshipTrainingViewModel> BuildApprenticeshipTrainingViewModel(
            bool isProvider, string courseId = null, string startDate = null)
        {
            var dates = await _startDateService.GetStartDates();

            var coursesResult = await _mediator.Send(new GetCoursesQuery());

            return new ApprenticeshipTrainingViewModel
            {
                RouteName = isProvider ? RouteNames.ProviderCreateApprenticeshipTraining : RouteNames.EmployerCreateApprenticeshipTraining,
                PossibleStartDates = dates.Select(startDateModel => new StartDateViewModel(startDateModel, startDate)).OrderBy(model => model.Value),
                Courses = coursesResult.Courses?.Select(course => new CourseViewModel(course, courseId)),
                CourseId = courseId,
                TrainingStartDate = startDate,
                IsProvider = isProvider
            };
        }
    }
}
