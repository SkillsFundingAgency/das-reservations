using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SFA.DAS.Authorization.CommitmentPermissions.Options;
using SFA.DAS.Authorization.Mvc.Attributes;
using SFA.DAS.Encoding;
using SFA.DAS.Reservations.Application.Employers.Queries.GetLegalEntities;
using SFA.DAS.Reservations.Application.Exceptions;
using SFA.DAS.Reservations.Application.Reservations.Commands.CacheReservationEmployer;
using SFA.DAS.Reservations.Application.Reservations.Commands.CreateReservationLevyEmployer;
using SFA.DAS.Reservations.Application.Reservations.Queries.GetAccountReservationStatus;
using SFA.DAS.Reservations.Application.Reservations.Queries.GetAvailableReservations;
using SFA.DAS.Reservations.Application.Reservations.Queries.GetProviderCacheReservationCommand;
using SFA.DAS.Reservations.Application.Reservations.Queries.GetReservation;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Web.Infrastructure;
using SFA.DAS.Reservations.Web.Models;

namespace SFA.DAS.Reservations.Web.Controllers
{
    [Authorize(Policy = nameof(PolicyNames.HasProviderOrEmployerAccount))]
    public class SelectReservationsController : Controller
    {
        private readonly IMediator _mediator;
        private readonly ILogger<ReservationsController> _logger;
        private readonly IEncodingService _encodingService;
        private readonly IExternalUrlHelper _urlHelper;

        public SelectReservationsController(IMediator mediator,
            ILogger<ReservationsController> logger,
            IEncodingService encodingService,
            IExternalUrlHelper urlHelper)
        {
            _mediator = mediator;
            _logger = logger;
            _encodingService = encodingService;
            _urlHelper = urlHelper;
        }


        [DasAuthorize(CommitmentOperation.AccessCohort)]
        [Route("{ukPrn}/reservations/{accountLegalEntityPublicHashedId}/select", Name = RouteNames.ProviderSelect)]
        [Route("accounts/{employerAccountId}/reservations/{accountLegalEntityPublicHashedId}/select", Name = RouteNames.EmployerSelect)]
        public async Task<IActionResult> SelectReservation(
            ReservationsRouteModel routeModel,
            SelectReservationViewModel viewModel)
        {
            var backUrl = string.Empty;

            try
            {
                var viewName = ViewNames.EmployerSelect;
                var apprenticeshipTrainingRouteName = RouteNames.EmployerApprenticeshipTraining;
                CacheReservationEmployerCommand cacheReservationEmployerCommand;

                if (routeModel.UkPrn.HasValue)
                {
                    backUrl = _urlHelper.GenerateUrl(new UrlParameters
                    {
                        Id = routeModel.UkPrn.Value.ToString(),
                        Controller = $"apprentices/{viewModel.CohortReference}",
                        Action = "details"
                    });

                    try
                    {
                        var response = await _mediator.Send(new GetProviderCacheReservationCommandQuery
                        {
                            AccountLegalEntityPublicHashedId = routeModel.AccountLegalEntityPublicHashedId,
                            CohortRef = routeModel.CohortRef,
                            CohortId = _encodingService.Decode(routeModel.CohortRef, EncodingType.CohortReference),
                            UkPrn = routeModel.UkPrn.Value
                        });

                        cacheReservationEmployerCommand = response.Command;

                    }
                    catch (AccountLegalEntityNotFoundException e)
                    {
                        _logger.LogWarning($"Account legal entity not found [{e.AccountLegalEntityPublicHashedId}].");
                        return RedirectToRoute(RouteNames.Error404);
                    }
                    catch (AccountLegalEntityInvalidException ex)
                    {
                        _logger.LogWarning(ex.Message);
                        return RedirectToRoute(RouteNames.Error500);
                    }


                    viewName = ViewNames.ProviderSelect;
                    apprenticeshipTrainingRouteName = RouteNames.ProviderApprenticeshipTraining;
                    try
                    {
                        var autoReserveStatus = await _mediator.Send(
                            new GetAccountReservationStatusQuery
                            {
                                AccountId = cacheReservationEmployerCommand.AccountId,
                                TransferSenderAccountId = viewModel.TransferSenderId ?? "",
                                HashedEmployerAccountId = _encodingService.Encode(cacheReservationEmployerCommand.AccountId, EncodingType.AccountId)
                            });

                        if (autoReserveStatus != null && autoReserveStatus.CanAutoCreateReservations)
                        {
                            var createdReservation = await _mediator.Send(new CreateReservationLevyEmployerCommand
                            {
                                AccountId = cacheReservationEmployerCommand.AccountId,
                                TransferSenderId = autoReserveStatus.TransferAccountId == 0 ? (long?)null : autoReserveStatus.TransferAccountId,
                                AccountLegalEntityId = _encodingService.Decode(
                                    cacheReservationEmployerCommand.AccountLegalEntityPublicHashedId,
                                    EncodingType.PublicAccountLegalEntityId)
                            });
                            
                            var addApprenticeUrl = _urlHelper.GenerateAddApprenticeUrl(createdReservation.ReservationId,
                                routeModel.AccountLegalEntityPublicHashedId, "", routeModel.UkPrn.Value, null,
                                viewModel.CohortReference);

                            return Redirect(addApprenticeUrl);
                        }
                    }
                    catch (TransferSendNotAllowedException e)
                    {
                        _logger.LogWarning(e, $"AccountId: {e.AccountId} does not have sender id {e.TransferSenderId} allowed).");
                        return RedirectToRoute(RouteNames.Error500);
                    }
                }
                else
                {
                    cacheReservationEmployerCommand = await BuildEmployerReservationCacheCommand(
                        routeModel.EmployerAccountId, routeModel.AccountLegalEntityPublicHashedId,
                        viewModel.CohortReference);

                    if (cacheReservationEmployerCommand == null)
                    {
                        _logger.LogWarning($"Account legal entity not found [{routeModel.AccountLegalEntityPublicHashedId}].");
                        return RedirectToRoute(RouteNames.Error500);
                    }
                }

                var availableReservationsResult = await _mediator.Send(
                    new GetAvailableReservationsQuery { AccountId = cacheReservationEmployerCommand.AccountId });

                if (availableReservationsResult.Reservations != null &&
                    availableReservationsResult.Reservations.Any())
                {
                    viewModel.AvailableReservations = availableReservationsResult.Reservations
                        .Select(reservation => new AvailableReservationViewModel(reservation));
                    viewModel.AccountId = cacheReservationEmployerCommand.AccountId;
                    viewModel.BackLink = backUrl;
                    return View(viewName, viewModel);
                }


                await _mediator.Send(cacheReservationEmployerCommand);

                routeModel.Id = cacheReservationEmployerCommand.Id;

                return RedirectToRoute(apprenticeshipTrainingRouteName, routeModel);
            }
            catch (ValidationException e)
            {
                _logger.LogWarning(e, "Validation error trying to render select reservation.");
                return RedirectToRoute(RouteNames.Error500);
            }
            catch (ProviderNotAuthorisedException e)
            {
                _logger.LogWarning(e, $"Provider (UKPRN: {e.UkPrn}) does not has access to create a reservation for legal entity for account (Id: {e.AccountId}).");
                return View("NoPermissions", backUrl);
            }
            catch (ReservationLimitReachedException)
            {
                return View("ReservationLimitReached", backUrl);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error trying to render select reservation.");
                return RedirectToRoute(RouteNames.Error500);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [DasAuthorize(CommitmentOperation.AccessCohort)]
        [Route("{ukPrn}/reservations/{accountLegalEntityPublicHashedId}/select", Name = RouteNames.ProviderSelect)]
        [Route("accounts/{employerAccountId}/reservations/{accountLegalEntityPublicHashedId}/select", Name = RouteNames.EmployerSelect)]
        public async Task<IActionResult> PostSelectReservation(
            ReservationsRouteModel routeModel,
            SelectReservationViewModel viewModel)
        {

            var backUrl = string.Empty;

            if (viewModel.SelectedReservationId == Guid.Empty)
            {
                var availableReservationsResult = await _mediator.Send(
                    new GetAvailableReservationsQuery { AccountId = viewModel.AccountId });

                viewModel.AvailableReservations = availableReservationsResult.Reservations
                    .Select(reservation => new AvailableReservationViewModel(reservation));

                ModelState.AddModelError(nameof(viewModel.SelectedReservationId), "Select an option");

                viewModel.BackLink = _urlHelper.GenerateUrl(new UrlParameters
                {
                    Id = routeModel.UkPrn.Value.ToString(),
                    Controller = $"apprentices/{viewModel.CohortReference}",
                    Action = "details"
                });

                return View("ProviderSelect", viewModel);
            }

            if (viewModel.SelectedReservationId.HasValue &&
                viewModel.SelectedReservationId != Guid.Empty &&
                viewModel.SelectedReservationId != Guid.Parse(Guid.Empty.ToString().Replace("0", "9")))
            {
                var reservation = await _mediator.Send(new GetReservationQuery { Id = viewModel.SelectedReservationId.Value });

                var url = _urlHelper.GenerateAddApprenticeUrl(viewModel.SelectedReservationId.Value,
                    routeModel.AccountLegalEntityPublicHashedId, reservation.Course.Id, routeModel.UkPrn.Value, reservation.StartDate,
                    viewModel.CohortReference);

                var addApprenticeUrl = url;

                return Redirect(addApprenticeUrl);
            }

            try
            {
                CacheReservationEmployerCommand cacheReservationEmployerCommand;

                if (routeModel.UkPrn.HasValue)
                {
                    backUrl = _urlHelper.GenerateUrl(new UrlParameters
                    {
                        Id = routeModel.UkPrn.Value.ToString(),
                        Controller = $"apprentices/{viewModel.CohortReference}",
                        Action = "details"
                    });

                    var response = await _mediator.Send(new GetProviderCacheReservationCommandQuery
                    {
                        AccountLegalEntityPublicHashedId = routeModel.AccountLegalEntityPublicHashedId,
                        CohortRef = routeModel.CohortRef,
                        CohortId = _encodingService.Decode(routeModel.CohortRef, EncodingType.CohortReference),
                        UkPrn = routeModel.UkPrn.Value
                    });

                    cacheReservationEmployerCommand = response.Command;
                }
                else
                {
                    cacheReservationEmployerCommand = await BuildEmployerReservationCacheCommand(
                        routeModel.EmployerAccountId, routeModel.AccountLegalEntityPublicHashedId,
                        viewModel.CohortReference);
                }

                await _mediator.Send(cacheReservationEmployerCommand);

                routeModel.Id = cacheReservationEmployerCommand.Id;
            }
            catch (ReservationLimitReachedException)
            {
                return View("ReservationLimitReached", backUrl);
            }
            catch (ProviderNotAuthorisedException e)
            {
                _logger.LogWarning(e, $"Provider (UKPRN: {e.UkPrn}) does not has access to create a reservation for legal entity for account (Id: {e.AccountId}).");
                return View("NoPermissions", backUrl);
            }

            return RedirectToRoute(RouteNames.ProviderApprenticeshipTraining, routeModel);
        }

        private async Task<CacheReservationEmployerCommand> BuildEmployerReservationCacheCommand(string employerAccountId, string accountLegalEntityPublicHashedId, string cohortRef)
        {
            var accountId = _encodingService.Decode(employerAccountId, EncodingType.AccountId);
            var accountLegalEntity = await _mediator.Send(new GetLegalEntitiesQuery { AccountId = accountId });
            var legalEntity = accountLegalEntity.AccountLegalEntities.SingleOrDefault(c =>
                c.AccountLegalEntityPublicHashedId.Equals(accountLegalEntityPublicHashedId));

            if (legalEntity == null)
            {
                return null;
            }

            return new CacheReservationEmployerCommand
            {
                AccountLegalEntityName = legalEntity.AccountLegalEntityName,
                AccountLegalEntityPublicHashedId = accountLegalEntityPublicHashedId,
                AccountId = accountId,
                AccountLegalEntityId = legalEntity.AccountLegalEntityId,
                Id = Guid.NewGuid(),
                CohortRef = cohortRef
            };
        }

    }


}