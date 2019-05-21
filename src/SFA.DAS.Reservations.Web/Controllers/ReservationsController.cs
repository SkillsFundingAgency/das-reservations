using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SFA.DAS.Encoding;
using SFA.DAS.Reservations.Application.Employers.Queries;
using SFA.DAS.Reservations.Application.Reservations.Commands.CacheReservationCourse;
using SFA.DAS.Reservations.Application.Reservations.Commands.CacheReservationStartDate;
using SFA.DAS.Reservations.Application.Reservations.Commands.CreateReservation;
using SFA.DAS.Reservations.Application.Reservations.Queries.GetCachedReservation;
using SFA.DAS.Reservations.Application.Reservations.Queries.GetCourses;
using SFA.DAS.Reservations.Application.Reservations.Queries.GetReservation;
using SFA.DAS.Reservations.Application.Reservations.Queries.GetReservations;
using SFA.DAS.Reservations.Domain.Courses;
using SFA.DAS.Reservations.Domain.Rules;
using SFA.DAS.Reservations.Infrastructure.Configuration;
using SFA.DAS.Reservations.Infrastructure.Exceptions;
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
        private readonly ILogger<ReservationsController> _logger;
        private readonly IEncodingService _encodingService;
        private readonly ReservationsWebConfiguration _configuration;

        public ReservationsController(
            IMediator mediator, 
            IStartDateService startDateService, 
            IOptions<ReservationsWebConfiguration> configuration,
            ILogger<ReservationsController> logger,
            IEncodingService encodingService)
        {
            _mediator = mediator;
            _startDateService = startDateService;
            _logger = logger;
            _encodingService = encodingService;
            _configuration = configuration.Value;
        }

        [Route("{ukPrn}/reservations/{id}/apprenticeship-training/{fromReview?}", Name = RouteNames.ProviderApprenticeshipTraining)]
        [Route("accounts/{employerAccountId}/reservations/{id}/apprenticeship-training", Name = RouteNames.EmployerApprenticeshipTraining)]
        public async Task<IActionResult> ApprenticeshipTraining(ReservationsRouteModel routeModel)
        {
            GetCachedReservationResult cachedReservation = null;

            if (routeModel.Id.HasValue)
            {
                cachedReservation = await _mediator.Send(new GetCachedReservationQuery {Id = routeModel.Id.GetValueOrDefault()});
                //todo: error handling if fails validation e.g. id not found, redirect to index.
            }
            
            var viewModel = await BuildApprenticeshipTrainingViewModel(
                routeModel.UkPrn != null, 
                cachedReservation?.AccountLegalEntityPublicHashedId, 
                cachedReservation?.CourseId, 
                cachedReservation?.StartDate, 
                routeModel.FromReview);

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
                var model = await BuildApprenticeshipTrainingViewModel(isProvider, null, formModel.SelectedCourseId, formModel.StartDate);//todo: need to get ale again.
                return View("ApprenticeshipTraining", model);
            }
            
            if (!string.IsNullOrWhiteSpace(formModel.StartDate))
                startDateModel = JsonConvert.DeserializeObject<StartDateModel>(formModel.StartDate);

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
		 		var cachedReservation = await _mediator.Send(new GetCachedReservationQuery {Id = routeModel.Id.GetValueOrDefault()});

                if (cachedReservation == null)
                {
                    throw new ArgumentException("Could not find reservation with given ID", nameof(routeModel));
                }
			
				if(isProvider)
				{             
	                var courseCommand = new CacheReservationCourseCommand
	                {
	                    Id = cachedReservation.Id,
	                    CourseId = course?.Id,
	                    UkPrn = routeModel.UkPrn.GetValueOrDefault()
	                };

	                await _mediator.Send(courseCommand);
				}

                var startDateCommand = new CacheReservationStartDateCommand
                {
                    Id = cachedReservation.Id,
                    StartDate = startDateModel?.StartDate.ToString("yyyy-MM"),
                    StartDateDescription = startDateModel?.ToString(),
                    UkPrn = routeModel.UkPrn.GetValueOrDefault()
                };

                await _mediator.Send(startDateCommand);
            }
            catch (ValidationException e)
            {
                foreach (var member in e.ValidationResult.MemberNames)
                {
                    ModelState.AddModelError(member.Split('|')[0], member.Split('|')[1]);
                }
                
                var model = await BuildApprenticeshipTrainingViewModel(isProvider, null, formModel.SelectedCourseId);//todo: add ale
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

            routeModel.FromReview = true;
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
                    Id = routeModel.Id.GetValueOrDefault(),
                    UkPrn = routeModel.UkPrn.GetValueOrDefault()
                };

                var result = await _mediator.Send(command);
                routeModel.AccountLegalEntityPublicHashedId = result.AccountLegalEntityPublicHashedId;
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning(ex, "Validation error when trying to create reservation from cached reservation.");
                return RedirectToRoute(routeModel.UkPrn.HasValue ? RouteNames.ProviderIndex : RouteNames.EmployerIndex, routeModel);
            }
            catch (CachedReservationNotFoundException ex)
            {
                _logger.LogWarning(ex, "Expected a cached reservation but did not find one.");
                return RedirectToRoute(routeModel.UkPrn.HasValue ? RouteNames.ProviderIndex : RouteNames.EmployerIndex, routeModel);
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
                Id = routeModel.Id.Value,
                UkPrn = routeModel.UkPrn.GetValueOrDefault()
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
                _configuration.ApprenticeUrl,
                _configuration.EmployerDashboardUrl
            );
            return View(model.ViewName, model);
        }

        [HttpPost]
        [Route("{ukPrn}/reservations/{id}/completed/{accountLegalEntityPublicHashedId}", Name = RouteNames.ProviderPostCompleted)]
        [Route("accounts/{employerAccountId}/reservations/{id}/completed/{accountLegalEntityPublicHashedId}", Name = RouteNames.EmployerPostCompleted)]
        public IActionResult PostCompleted(ReservationsRouteModel routeModel, ConfirmationRedirectViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var viewModel = new CompletedViewModel
                (
                    model.ReservationId,
                    model.StartDate,
                    model.ExpiryDate,
                    new Course(model.CourseId, model.CourseTitle, model.Level),
                    routeModel.AccountLegalEntityPublicHashedId,
                    routeModel.UkPrn,
                    model.AccountLegalEntityName,
                    _configuration.DashboardUrl,
                    _configuration.ApprenticeUrl,
                    _configuration.EmployerDashboardUrl
                );
                
                return View(viewModel.ViewName, viewModel);
            }

            if (model.AddApprentice.HasValue && model.AddApprentice.Value)
            {
                return Redirect(model.ApprenticeUrl);
            }

            return Redirect(model.DashboardUrl);
        }

        [Route("{ukPrn}/reservations/manage", Name = RouteNames.ProviderManage)]
        [Route("accounts/{employerAccountId}/reservations/manage", Name = RouteNames.EmployerManage)]
        public async Task<IActionResult> Manage(ReservationsRouteModel routeModel)
        {
            var employerAccountIds = new List<long>();
            var reservations = new List<ReservationViewModel>();
            string viewName;

            if (routeModel.UkPrn.HasValue)
            {
                var trustedEmployersResponse = await _mediator.Send(new GetTrustedEmployersQuery { UkPrn = routeModel.UkPrn.Value });
                employerAccountIds.AddRange(trustedEmployersResponse.Employers.Select(employer => employer.AccountId));
                viewName = ViewNames.ProviderManage;
            }
            else
            {
                var decodedAccountId = _encodingService.Decode(routeModel.EmployerAccountId, EncodingType.AccountId);
                employerAccountIds.Add(decodedAccountId);
                viewName = ViewNames.EmployerManage;
            }

            foreach (var employerAccountId in employerAccountIds)
            {
                var reservationsResult = await _mediator.Send(new GetReservationsQuery{AccountId = employerAccountId});

                foreach (var reservation in reservationsResult.Reservations)
                {
                    if (!reservation.ProviderId.HasValue || reservation.ProviderId == 0)
                    {
                        reservation.ProviderId = routeModel.UkPrn;
                    }

                    var viewModel = new ReservationViewModel(
                        reservation, 
                        _configuration.ApprenticeUrl, 
                        _encodingService.Encode(reservation.AccountLegalEntityId, EncodingType.PublicAccountLegalEntityId));

                    reservations.Add(viewModel);
                }
            }
            
            return View(viewName, new ManageViewModel{Reservations = reservations});
        }

        private async Task<ApprenticeshipTrainingViewModel> BuildApprenticeshipTrainingViewModel(
            bool isProvider,
            string accountLegalEntityPublicHashedId,
            string courseId = null, 
            string startDate = null, 
            bool? routeModelFromReview = false)
        {
            var accountLegalEntityId = _encodingService.Decode(
                accountLegalEntityPublicHashedId,
                EncodingType.PublicAccountLegalEntityId);
            var dates = await _startDateService.GetStartDates(accountLegalEntityId);

            var coursesResult = await _mediator.Send(new GetCoursesQuery());

            return new ApprenticeshipTrainingViewModel
            {
                RouteName = isProvider ? RouteNames.ProviderCreateApprenticeshipTraining : RouteNames.EmployerCreateApprenticeshipTraining,
                PossibleStartDates = dates.Select(startDateModel => new StartDateViewModel(startDateModel, startDate)).OrderBy(model => model.Value),
                Courses = coursesResult.Courses?.Select(course => new CourseViewModel(course, courseId)),
                CourseId = courseId,
                TrainingStartDate = startDate,
                IsProvider = isProvider,
                BackLink = isProvider ?
                    routeModelFromReview.HasValue && routeModelFromReview.Value ? RouteNames.ProviderReview : RouteNames.ProviderConfirmEmployer 
                    : routeModelFromReview.HasValue && routeModelFromReview.Value ? RouteNames.EmployerReview : RouteNames.EmployerSelectCourse 
            };
        }
    }
}
