﻿using System;
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
            var backUrl = _urlHelper.GenerateCommitmentsUrl(new UrlParameters
            {
                Id = routeModel.UkPrn.HasValue ? routeModel.UkPrn.Value.ToString() : routeModel.EmployerAccountId,
                Controller = $"apprentices/{viewModel.CohortReference}",
                Action = "details",
                Folder = routeModel.UkPrn.HasValue ? "" : "commitments/accounts"
            });

            try
            {
                var apprenticeshipTrainingRouteName = RouteNames.EmployerSelectCourse;
                CacheReservationEmployerCommand cacheReservationEmployerCommand;

                if (routeModel.UkPrn.HasValue)
                {
                    
                    try
                    {
                        var response = await _mediator.Send(new GetProviderCacheReservationCommandQuery
                        {
                            AccountLegalEntityPublicHashedId = routeModel.AccountLegalEntityPublicHashedId,
                            CohortRef = routeModel.CohortReference,
                            CohortId = _encodingService.Decode(routeModel.CohortReference, EncodingType.CohortReference),
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

                    apprenticeshipTrainingRouteName = RouteNames.ProviderApprenticeshipTraining;

                    
                }
                else
                {
                    cacheReservationEmployerCommand = await BuildEmployerReservationCacheCommand(
                        routeModel.EmployerAccountId, routeModel.AccountLegalEntityPublicHashedId,
                        viewModel.CohortReference);

                    if (cacheReservationEmployerCommand == null)
                    {
                        _logger.LogWarning(
                            $"Account legal entity not found [{routeModel.AccountLegalEntityPublicHashedId}].");
                        return RedirectToRoute(RouteNames.Error500);
                    }
                }

                var redirectResult = await CheckCanAutoReserve(cacheReservationEmployerCommand.AccountId,
                    viewModel.TransferSenderId, cacheReservationEmployerCommand.AccountLegalEntityPublicHashedId,
                    routeModel.UkPrn, viewModel.CohortReference, routeModel.EmployerAccountId);
                if (!string.IsNullOrEmpty(redirectResult))
                {
                    if (redirectResult == RouteNames.Error500)
                    {
                        return RedirectToRoute(redirectResult);
                    }

                    return Redirect(redirectResult);
                }

                var availableReservationsResult = await _mediator.Send(
                    new GetAvailableReservationsQuery {AccountId = cacheReservationEmployerCommand.AccountId});

                if (availableReservationsResult.Reservations != null &&
                    availableReservationsResult.Reservations.Any())
                {
                    viewModel.AvailableReservations = availableReservationsResult.Reservations
                        .Select(reservation => new AvailableReservationViewModel(reservation));
                    viewModel.AccountId = cacheReservationEmployerCommand.AccountId;
                    viewModel.BackLink = backUrl;
                    return View(ViewNames.Select, viewModel);
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
                _logger.LogWarning(e,
                    $"Provider (UKPRN: {e.UkPrn}) does not has access to create a reservation for legal entity for account (Id: {e.AccountId}).");
                return View("NoPermissions", backUrl);
            }
            catch (ReservationLimitReachedException)
            {
                return View("ReservationLimitReached", backUrl);
            }
            catch (GlobalReservationRuleException)
            {
                if (routeModel.UkPrn.HasValue)
                {
                    return View("ProviderFundingPaused");
                }
                return View("EmployerFundingPaused");
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

            var backUrl = _urlHelper.GenerateCommitmentsUrl(new UrlParameters
            {
                Id = routeModel.UkPrn.HasValue ? routeModel.UkPrn.Value.ToString() : routeModel.EmployerAccountId,
                Controller = $"apprentices/{routeModel.CohortReference}",
                Action = "details",
                Folder = routeModel.UkPrn.HasValue ? "" : "commitments/accounts"
            });

            if (!viewModel.SelectedReservationId.HasValue || viewModel.SelectedReservationId == Guid.Empty)
            {
                var availableReservationsResult = await _mediator.Send(
                    new GetAvailableReservationsQuery { AccountId = viewModel.AccountId });

                viewModel.AvailableReservations = availableReservationsResult.Reservations
                    .Select(reservation => new AvailableReservationViewModel(reservation));

                ModelState.AddModelError(nameof(viewModel.SelectedReservationId), "Select an option");

                viewModel.BackLink = backUrl;

                return View(ViewNames.Select, viewModel);
            }

            if (viewModel.SelectedReservationId != Guid.Empty &&
                viewModel.SelectedReservationId != Guid.Parse(Guid.Empty.ToString().Replace("0", "9")))
            {
                var reservation = await _mediator.Send(new GetReservationQuery { Id = viewModel.SelectedReservationId.Value });

                var url = _urlHelper.GenerateAddApprenticeUrl(viewModel.SelectedReservationId.Value,
                    routeModel.AccountLegalEntityPublicHashedId, reservation.Course.Id, routeModel.UkPrn, reservation.StartDate,
                    routeModel.CohortReference, routeModel.EmployerAccountId);

                var addApprenticeUrl = url;

                return Redirect(addApprenticeUrl);
            }

            try
            {
                CacheReservationEmployerCommand cacheReservationEmployerCommand;

                if (routeModel.UkPrn.HasValue)
                {
                    var response = await _mediator.Send(new GetProviderCacheReservationCommandQuery
                    {
                        AccountLegalEntityPublicHashedId = routeModel.AccountLegalEntityPublicHashedId,
                        CohortRef = routeModel.CohortReference,
                        CohortId = _encodingService.Decode(routeModel.CohortReference, EncodingType.CohortReference),
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
            catch (GlobalReservationRuleException)
            {
                if (routeModel.UkPrn.HasValue)
                {
                    return View("ProviderFundingPaused");
                }
                return View("EmployerFundingPaused");
            }
            var routeName = RouteNames.ProviderApprenticeshipTraining;
            if (!routeModel.UkPrn.HasValue)
            {
                routeName = RouteNames.EmployerSelectCourse;
            }
            
            return RedirectToRoute(routeName, routeModel);
        }

        private async Task<string> CheckCanAutoReserve(long accountId, string transferSenderId,string accountLegalEntityPublicHashedId, uint? ukPrn,string cohortRef, string hashedAccountId)
        {
            try
            {
                var hashedEmployerAccountId = _encodingService.Encode(accountId, EncodingType.AccountId);
                var autoReserveStatus = await _mediator.Send(
                    new GetAccountReservationStatusQuery
                    {
                        AccountId = accountId,
                        TransferSenderAccountId = transferSenderId ?? "",
                        HashedEmployerAccountId = hashedEmployerAccountId
                    });

                if (autoReserveStatus != null && autoReserveStatus.CanAutoCreateReservations)
                {
                    var createdReservation = await _mediator.Send(new CreateReservationLevyEmployerCommand
                    {
                        AccountId = accountId,
                        TransferSenderId = autoReserveStatus.TransferAccountId == 0 ? (long?)null : autoReserveStatus.TransferAccountId,
                        AccountLegalEntityId = _encodingService.Decode(
                            accountLegalEntityPublicHashedId,
                            EncodingType.PublicAccountLegalEntityId)
                    });

                    var addApprenticeUrl = _urlHelper.GenerateAddApprenticeUrl(createdReservation.ReservationId,
                        accountLegalEntityPublicHashedId, "", ukPrn, null,
                        cohortRef, hashedAccountId);

                    return addApprenticeUrl;
                }
            }
            catch (TransferSendNotAllowedException e)
            {
                _logger.LogWarning(e, $"AccountId: {e.AccountId} does not have sender id {e.TransferSenderId} allowed).");
                return RouteNames.Error500;
            }

            return string.Empty;
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