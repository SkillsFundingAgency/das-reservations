using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.Extensions.Logging;
using SFA.DAS.Encoding;
using SFA.DAS.Reservations.Application.Exceptions;
using SFA.DAS.Reservations.Application.Reservations.Commands.DeleteReservation;
using SFA.DAS.Reservations.Application.Reservations.Queries.GetReservation;
using SFA.DAS.Reservations.Application.Reservations.Queries.GetReservations;
using SFA.DAS.Reservations.Application.Reservations.Queries.SearchReservations;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Domain.Reservations;
using SFA.DAS.Reservations.Web.Filters;
using SFA.DAS.Reservations.Web.Infrastructure;
using SFA.DAS.Reservations.Web.Models;

namespace SFA.DAS.Reservations.Web.Controllers
{
    [Authorize(Policy = nameof(PolicyNames.HasEmployerViewerUserRoleOrIsProvider))]
    [ServiceFilter(typeof(LevyNotPermittedFilter))]
    public class ManageReservationsController : Controller
    {
        private readonly IMediator _mediator;
        private readonly IEncodingService _encodingService;
        private readonly IExternalUrlHelper _urlHelper;
        private readonly ISessionStorageService<ManageReservationsFilterModelBase> _sessionStorageService;
        private readonly ILogger<ManageReservationsController> _logger;

        public ManageReservationsController(
            IMediator mediator,
            IEncodingService encodingService,
            IExternalUrlHelper urlHelper,
            ISessionStorageService<ManageReservationsFilterModelBase> sessionStorageService,
            ILogger<ManageReservationsController> logger)
        {
            _mediator = mediator;
            _encodingService = encodingService;
            _urlHelper = urlHelper;
            _sessionStorageService = sessionStorageService;
            _logger = logger;
        }

        [Route("accounts/{employerAccountId}/reservations/manage", Name = RouteNames.EmployerManage)]
        public async Task<IActionResult> EmployerManage(ReservationsRouteModel routeModel)
        {
            var reservations = new List<ReservationViewModel>();

            var decodedAccountId = _encodingService.Decode(routeModel.EmployerAccountId, EncodingType.AccountId);
            var reservationsResult = await _mediator.Send(new GetReservationsQuery{AccountId = decodedAccountId});

            foreach (var reservation in reservationsResult.Reservations)
            {
                var accountLegalEntityPublicHashedId = _encodingService.Encode(reservation.AccountLegalEntityId,
                    EncodingType.PublicAccountLegalEntityId);

                var apprenticeUrl = reservation.Status == ReservationStatus.Pending && !reservation.IsExpired
                    ? _urlHelper.GenerateAddApprenticeUrl(
                    reservation.Id,
                    accountLegalEntityPublicHashedId,
                    reservation.Course.Id,
                    routeModel.UkPrn,
                    reservation.StartDate,
                    routeModel.CohortReference,
                    routeModel.EmployerAccountId)
                    : string.Empty;

                var viewModel = new ReservationViewModel(reservation, apprenticeUrl, routeModel.UkPrn);
                reservations.Add(viewModel);
            }

            return View(ViewNames.EmployerManage, new ManageViewModel
            {
                Reservations = reservations,
                BackLink = _urlHelper.GenerateDashboardUrl(routeModel.EmployerAccountId)
            });
        }

        [Route("{ukPrn}/reservations/manage", Name = RouteNames.ProviderManage)]
        public async Task<IActionResult> ProviderManage(ReservationsRouteModel routeModel, ManageReservationsFilterModel filterModel)
        {
            try
            {
                if (routeModel.IsFromManage.HasValue && routeModel.IsFromManage.Value)
                {
                    var storedSearch = _sessionStorageService.Get();
                    if (storedSearch != null)
                    {
                        filterModel.SearchTerm = storedSearch.SearchTerm;
                        filterModel.SelectedCourse = storedSearch.SelectedCourse;
                        filterModel.SelectedEmployer = storedSearch.SelectedEmployer;
                        filterModel.SelectedStartDate = storedSearch.SelectedStartDate;
                        filterModel.PageNumber = storedSearch.PageNumber;
                    }
                    routeModel.IsFromManage = false;
                }
                else
                {
                    _sessionStorageService.Delete();
                }

                var reservations = new List<ReservationViewModel>();

                var searchResult = await _mediator.Send(new SearchReservationsQuery
                {
                    ProviderId = routeModel.UkPrn.Value,
                    Filter = filterModel
                });
                filterModel.NumberOfRecordsFound = searchResult.NumberOfRecordsFound;
                filterModel.EmployerFilters = searchResult.EmployerFilters;
                filterModel.CourseFilters = searchResult.CourseFilters;
                filterModel.StartDateFilters = searchResult.StartDateFilters;

                foreach (var reservation in searchResult.Reservations)
                {
                    var accountLegalEntityPublicHashedId = _encodingService.Encode(reservation.AccountLegalEntityId,
                        EncodingType.PublicAccountLegalEntityId);

                    var apprenticeUrl = reservation.Status == ReservationStatus.Pending && !reservation.IsExpired
                        ? _urlHelper.GenerateAddApprenticeUrl(
                            reservation.Id,
                            accountLegalEntityPublicHashedId,
                            reservation.Course.Id,
                            routeModel.UkPrn,
                            reservation.StartDate,
                            routeModel.CohortReference,
                            routeModel.EmployerAccountId)
                        : string.Empty;

                    var viewModel = new ReservationViewModel(reservation, apprenticeUrl, routeModel.UkPrn);

                    reservations.Add(viewModel);
                }

                if (!string.IsNullOrEmpty(filterModel.SearchTerm)
                    || filterModel.PageNumber != 1
                    || !string.IsNullOrEmpty(filterModel.SelectedCourse)
                    || !string.IsNullOrEmpty(filterModel.SelectedEmployer)
                    || !string.IsNullOrEmpty(filterModel.SelectedStartDate))
                {
                    _sessionStorageService.Store(filterModel);
                }


                return View(ViewNames.ProviderManage, new ManageViewModel
                {
                    Reservations = reservations,
                    BackLink = _urlHelper.GenerateDashboardUrl(routeModel.EmployerAccountId),
                    FilterModel = filterModel,
                    TotalReservationCount = searchResult.TotalReservationsForProvider
                });
            }
            catch (ProviderNotAuthorisedException e)
            {
                _logger.LogInformation(e, $"Provider {e.UkPrn} has no permissions for viewing manage");
                return View("NoPermissions");
            }
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

                    routeModel.IsFromManage = true;
                    
                    return RedirectToRoute(manageRoute, routeModel);
                }

                await _mediator.Send(new DeleteReservationCommand{ReservationId = routeModel.Id.Value, DeletedByEmployer = !isProvider});

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
            var dashboardUrl = _urlHelper.GenerateDashboardUrl(routeModel.EmployerAccountId);

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
    }
}