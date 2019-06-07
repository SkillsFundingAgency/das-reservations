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
using SFA.DAS.Reservations.Application.Reservations.Commands.DeleteReservation;
using SFA.DAS.Reservations.Application.Reservations.Queries.GetCachedReservation;
using SFA.DAS.Reservations.Application.Reservations.Queries.GetCourses;
using SFA.DAS.Reservations.Application.Reservations.Queries.GetReservation;
using SFA.DAS.Reservations.Application.Reservations.Queries.GetReservations;
using SFA.DAS.Reservations.Domain.Courses;
using SFA.DAS.Reservations.Domain.Interfaces;
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
        private readonly IExternalUrlHelper _urlHelper;

        public ReservationsController(
            IMediator mediator, 
            IStartDateService startDateService, 
            IOptions<ReservationsWebConfiguration> configuration,
            ILogger<ReservationsController> logger,
            IEncodingService encodingService,
            IExternalUrlHelper urlHelper)
        {
            _mediator = mediator;
            _startDateService = startDateService;
            _logger = logger;
            _encodingService = encodingService;
            _configuration = configuration.Value;
            _urlHelper = urlHelper;
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
            Course course = null;

            try
            {
                if (!string.IsNullOrWhiteSpace(formModel.StartDate))
                    startDateModel = JsonConvert.DeserializeObject<StartDateModel>(formModel.StartDate);

                if (!ModelState.IsValid)
                {
                    var model = await BuildApprenticeshipTrainingViewModel(
                        isProvider, 
                        formModel.AccountLegalEntityPublicHashedId, 
                        formModel.SelectedCourseId, 
                        startDateModel?.StartDate.ToString("yyyy-MM"));
                    return View("ApprenticeshipTraining", model);
                }

                if (!string.IsNullOrEmpty(formModel.SelectedCourseId))
                {
                    var getCoursesResult = await _mediator.Send(new GetCoursesQuery());

                    var selectedCourse =
                        getCoursesResult.Courses.SingleOrDefault(c => c.Id.Equals(formModel.SelectedCourseId));

                    course = selectedCourse ?? throw new ArgumentException("Selected course does not exist", nameof(formModel.SelectedCourseId));
                    //todo: should be a validation exception, also this throw is not unit tested
                }

		 		var cachedReservation = await _mediator.Send(new GetCachedReservationQuery {Id = routeModel.Id.GetValueOrDefault()});

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
                
                var model = await BuildApprenticeshipTrainingViewModel(
                    isProvider, 
                    formModel.AccountLegalEntityPublicHashedId, 
                    formModel.SelectedCourseId,
                    formModel.StartDate);
                return View("ApprenticeshipTraining", model);
            }
            catch (CachedReservationNotFoundException ex)
            {
                _logger.LogWarning(ex, "Expected a cached reservation but did not find one.");
                return RedirectToRoute(routeModel.UkPrn.HasValue ? RouteNames.ProviderIndex : RouteNames.EmployerIndex, routeModel);
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
                _configuration.EmployerDashboardUrl,
                _urlHelper.GenerateUrl(subDomain: "recruit", id: routeModel.UkPrn.ToString())

            );
            return View(model.ViewName, model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
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
                    _configuration.EmployerDashboardUrl,
                    model.RecruitApprenticeUrl
                    
                );
                
                return View(viewModel.ViewName, viewModel);
            }

            if (!string.IsNullOrEmpty(model.WhatsNext) && !string.IsNullOrWhiteSpace(model.WhatsNext))
            {

                switch (model.WhatsNext)
                {
                    case ConfirmationRedirectViewModel.RedirectOptions.RecruitAnApprentice:
                        return Redirect(model.RecruitApprenticeUrl);

                    case ConfirmationRedirectViewModel.RedirectOptions.AddAnApprentice:
                        return Redirect(model.ApprenticeUrl);

                    case ConfirmationRedirectViewModel.RedirectOptions.ProviderHomepage:
                        return Redirect(model.DashboardUrl);
                }
            }

            ModelState.AddModelError("WhatsNext", "Select what you would like to do next");
            return PostCompleted(routeModel, model);
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

                if (!trustedEmployersResponse.Employers.Any())
                {
                    return View("NoPermissions");
                }

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
                    if (routeModel.UkPrn.HasValue)
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

        [Route("{ukPrn}/reservations/{id}/delete", Name = RouteNames.ProviderDelete)]
        [Route("accounts/{employerAccountId}/reservations/{id}/delete", Name = RouteNames.EmployerDelete)]
        public async Task<IActionResult> Delete(ReservationsRouteModel routeModel)
        {
            var isProvider = routeModel.UkPrn.HasValue;
            try
            {
                if (!routeModel.Id.HasValue)
                {
                    _logger.LogInformation($"Reservation ID must be in URL, parameter [{nameof(routeModel.Id)}]");
                    var manageRoute = isProvider ? RouteNames.ProviderManage : RouteNames.EmployerManage;
                    return RedirectToRoute(manageRoute, routeModel);
                }

                var query = new GetReservationQuery
                {
                    Id = routeModel.Id.Value,
                    UkPrn = routeModel.UkPrn.GetValueOrDefault()
                };
                var queryResult = await _mediator.Send(query);

                var viewName = isProvider ? ViewNames.ProviderDelete : ViewNames.EmployerDelete;

                return View(viewName, new DeleteViewModel(queryResult));
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error preparing for the delete view");
                throw;
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("{ukPrn}/reservations/{id}/delete", Name = RouteNames.ProviderDelete)]
        [Route("accounts/{employerAccountId}/reservations/{id}/delete", Name = RouteNames.EmployerDelete)]
        public async Task<IActionResult> PostDelete(ReservationsRouteModel routeModel, DeleteViewModel viewModel)
        {
            var isProvider = routeModel.UkPrn.HasValue;
            var deleteViewName = isProvider ? ViewNames.ProviderDelete : ViewNames.EmployerDelete;
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(deleteViewName, viewModel);
                }

                if (viewModel.Delete.HasValue && !viewModel.Delete.Value ||
                    !routeModel.Id.HasValue)
                {
                    var manageRoute = isProvider ? RouteNames.ProviderManage : RouteNames.EmployerManage;
                    return RedirectToRoute(manageRoute, routeModel);
                }
                
                await _mediator.Send(new DeleteReservationCommand{ReservationId = routeModel.Id.Value});

                var completedRoute = isProvider ? RouteNames.ProviderDeleteCompleted : RouteNames.EmployerDeleteCompleted;
                return RedirectToRoute(completedRoute, routeModel);
            }
            catch (ValidationException ex)
            {
                _logger.LogInformation(ex, $"Validation error trying to delete reservation [{routeModel.Id}]");
                return View(deleteViewName, viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error trying to delete reservation [{routeModel.Id}]");
                var errorRoute = isProvider ? RouteNames.ProviderError : RouteNames.EmployerError;
                return RedirectToRoute(errorRoute, routeModel);
            }
        }

        [Route("{ukPrn}/reservations/{id}/delete-completed", Name = RouteNames.ProviderDeleteCompleted)]
        [Route("accounts/{employerAccountId}/reservations/{id}/delete-completed", Name = RouteNames.EmployerDeleteCompleted)]
        public IActionResult DeleteCompleted(ReservationsRouteModel routeModel)
        {
            var viewName = routeModel.UkPrn.HasValue ? ViewNames.ProviderDeleteCompleted : ViewNames.EmployerDeleteCompleted;

            return View(viewName);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("{ukPrn}/reservations/{id}/delete-completed", Name = RouteNames.ProviderDeleteCompleted)]
        [Route("accounts/{employerAccountId}/reservations/{id}/delete-completed", Name = RouteNames.EmployerDeleteCompleted)]
        public IActionResult PostDeleteCompleted(ReservationsRouteModel routeModel, DeleteCompletedViewModel viewModel)
        {
            var isProvider = routeModel.UkPrn.HasValue;
            var deleteCompletedViewName = isProvider ? ViewNames.ProviderDeleteCompleted : ViewNames.EmployerDeleteCompleted;
            var manageRouteName = isProvider ? RouteNames.ProviderManage : RouteNames.EmployerManage;
            var dashboardUrl = isProvider ? _configuration.DashboardUrl : _configuration.EmployerDashboardUrl;

            if (!ModelState.IsValid)
            {
                return View(deleteCompletedViewName, viewModel);
            }

            if (viewModel.Manage.HasValue && viewModel.Manage.Value)
            {
                return RedirectToRoute(manageRouteName);
            }

            return Redirect(dashboardUrl);
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
                AccountLegalEntityPublicHashedId = accountLegalEntityPublicHashedId,
                TrainingStartDate = startDate,
                IsProvider = isProvider,
                BackLink = isProvider ?
                    routeModelFromReview.HasValue && routeModelFromReview.Value ? RouteNames.ProviderReview : RouteNames.ProviderConfirmEmployer 
                    : routeModelFromReview.HasValue && routeModelFromReview.Value ? RouteNames.EmployerReview : RouteNames.EmployerSelectCourse 
            };
        }
    }
}
