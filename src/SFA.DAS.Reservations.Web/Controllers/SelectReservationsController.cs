using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SFA.DAS.Encoding;
using SFA.DAS.Reservations.Application.Employers.Queries.GetLegalEntities;
using SFA.DAS.Reservations.Application.Exceptions;
using SFA.DAS.Reservations.Application.Reservations.Commands.CacheReservationEmployer;
using SFA.DAS.Reservations.Application.Reservations.Commands.CreateReservationLevyEmployer;
using SFA.DAS.Reservations.Application.Reservations.Queries.GetAvailableReservations;
using SFA.DAS.Reservations.Application.Reservations.Queries.GetProviderCacheReservationCommand;
using SFA.DAS.Reservations.Application.Reservations.Queries.GetReservation;
using SFA.DAS.Reservations.Domain.Employers;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Infrastructure.Services;
using SFA.DAS.Reservations.Web.Infrastructure;
using SFA.DAS.Reservations.Web.Models;
using SFA.DAS.Reservations.Web.Services;

namespace SFA.DAS.Reservations.Web.Controllers;

[Authorize(Policy = nameof(PolicyNames.HasProviderOrEmployerAccount))]
public class SelectReservationsController : Controller
{
    private readonly IMediator _mediator;
    private readonly ILogger<ReservationsController> _logger;
    private readonly IEncodingService _encodingService;
    private readonly IExternalUrlHelper _urlHelper;
    private readonly IConfiguration _configuration;
    private readonly IUserClaimsService _userClaimsService;

    public SelectReservationsController(IMediator mediator,
        ILogger<ReservationsController> logger,
        IEncodingService encodingService,
        IConfiguration configuration,
        IExternalUrlHelper urlHelper,
        IUserClaimsService userClaimsService)
    {
        _mediator = mediator;
        _logger = logger;
        _encodingService = encodingService;
        _urlHelper = urlHelper;
        _userClaimsService = userClaimsService;
        _configuration = configuration;
    }

    [Authorize(Policy = nameof(PolicyNames.AccessCohort))]
    [Route("{ukPrn}/reservations/{accountLegalEntityPublicHashedId}/select", Name = RouteNames.ProviderSelect)]
    [Route("accounts/{employerAccountId}/reservations/{accountLegalEntityPublicHashedId}/select", Name = RouteNames.EmployerSelect)]
    public async Task<IActionResult> SelectReservation(
        ReservationsRouteModel routeModel,
        SelectReservationViewModel viewModel)
    {
        var backUrl = GetBackUrl(routeModel, viewModel);
        try
        {
            var apprenticeshipTrainingRouteName = RouteNames.EmployerSelectCourseRuleCheck;
            CacheReservationEmployerCommand cacheReservationEmployerCommand;
            Guid? userId = null;
            if (routeModel.UkPrn.HasValue)
            {
                var response = await _mediator.Send(new GetProviderCacheReservationCommandQuery
                {
                    AccountLegalEntityPublicHashedId = routeModel.AccountLegalEntityPublicHashedId,
                    CohortRef = routeModel.CohortReference,
                    CohortId = GetCohortId(routeModel.CohortReference),
                    UkPrn = routeModel.UkPrn.Value
                });

                cacheReservationEmployerCommand = response.Command;

                apprenticeshipTrainingRouteName = RouteNames.ProviderApprenticeshipTrainingRuleCheck;
            }
            else
            {
                var userAccountIdClaim = HttpContext.User.Claims.First(c =>
                    c.Type.Equals(EmployerClaims.IdamsUserIdClaimTypeIdentifier));
                userId = Guid.Parse(userAccountIdClaim.Value);

                cacheReservationEmployerCommand = await BuildEmployerReservationCacheCommand(
                    routeModel.EmployerAccountId, routeModel.AccountLegalEntityPublicHashedId,
                    viewModel.CohortReference, viewModel.ProviderId, viewModel.JourneyData);
            }

            var redirectResult = await CheckCanAutoReserve(cacheReservationEmployerCommand.AccountId,
                viewModel.TransferSenderId, viewModel.JourneyData,
                cacheReservationEmployerCommand.AccountLegalEntityPublicHashedId,
                routeModel.UkPrn ?? viewModel.ProviderId, viewModel.CohortReference,
                routeModel.EmployerAccountId, userId, viewModel.EncodedPledgeApplicationId);
            if (!string.IsNullOrEmpty(redirectResult))
            {
                if (redirectResult == RouteNames.Error500)
                {
                    return RedirectToRoute(redirectResult);
                }

                return Redirect(redirectResult);
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
                return View(ViewNames.Select, viewModel);
            }

            if (IsThisAnEmployer() && string.IsNullOrWhiteSpace(viewModel.CohortReference))
            {
                var continueRoute = _urlHelper.GenerateAddApprenticeUrl(null,
                    routeModel.AccountLegalEntityPublicHashedId, "", viewModel.ProviderId, null,
                    viewModel.CohortReference, routeModel.EmployerAccountId, string.IsNullOrEmpty(viewModel.CohortReference) && IsThisAnEmployer(),
                    "", viewModel.EncodedPledgeApplicationId, viewModel.JourneyData);

                return Redirect(continueRoute);
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
                return View("ProviderFundingPaused", backUrl);
            }

            return View("EmployerFundingPaused", backUrl);
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
        catch (TransferSenderNotAllowedException e)
        {
            _logger.LogWarning(e,
                $"AccountId: {e.AccountId} does not have sender id {e.TransferSenderId} allowed).");
            return RedirectToRoute(RouteNames.Error500);
        }
        catch (EmployerAgreementNotSignedException e)
        {
            _logger.LogWarning(e, $"AccountId: {e.AccountId} does not have a signed agreement for ALE {e.AccountLegalEntityId}).");

            var routeName = RouteNames.EmployerTransactorSignAgreement;
            if (routeModel.UkPrn.HasValue)
            {
                routeName = RouteNames.ProviderEmployerAgreementNotSigned;
            }
            else
            {
                if (_userClaimsService.UserIsInRole(routeModel.EmployerAccountId,
                        EmployerUserRole.Owner, User.Claims))
                {
                    routeName = RouteNames.EmployerOwnerSignAgreement;
                }
            }

            routeModel.IsFromSelect = true;
            routeModel.PreviousPage = backUrl;

            return RedirectToRoute(routeName, routeModel);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error trying to render select reservation.");
            return RedirectToRoute(RouteNames.Error500);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = nameof(PolicyNames.AccessCohort))]
    [Route("{ukPrn}/reservations/{accountLegalEntityPublicHashedId}/select", Name = RouteNames.ProviderSelect)]
    [Route("accounts/{employerAccountId}/reservations/{accountLegalEntityPublicHashedId}/select", Name = RouteNames.EmployerSelect)]
    public async Task<IActionResult> PostSelectReservation(
        ReservationsRouteModel routeModel,
        SelectReservationViewModel viewModel)
    {
        _logger.LogInformation("{TypeName} routeModel: {Model}", nameof(SelectReservationsController), JsonConvert.SerializeObject(routeModel));
        var createViaAutoReservation = false;
        var backUrl = GetBackUrl(routeModel, viewModel);
        var isEmployerSelect = IsThisAnEmployer();

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
                routeModel.AccountLegalEntityPublicHashedId, reservation.Course.Id, routeModel.UkPrn ?? viewModel.ProviderId, reservation.StartDate,
                viewModel.CohortReference, routeModel.EmployerAccountId, string.IsNullOrEmpty(viewModel.CohortReference) && isEmployerSelect, journeyData: viewModel.JourneyData);

            var addApprenticeUrl = url;

            return Redirect(addApprenticeUrl);
        }

        if (isEmployerSelect && viewModel.SelectedReservationId == Guid.Parse(Guid.Empty.ToString().Replace("0", "9")))
        {
            createViaAutoReservation = true;
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
                    CohortId = GetCohortId(routeModel.CohortReference),
                    UkPrn = routeModel.UkPrn.Value
                });

                cacheReservationEmployerCommand = response.Command;
            }
            else
            {
                cacheReservationEmployerCommand = await BuildEmployerReservationCacheCommand(
                    routeModel.EmployerAccountId, routeModel.AccountLegalEntityPublicHashedId,
                    viewModel.CohortReference, viewModel.ProviderId, viewModel.JourneyData);
                cacheReservationEmployerCommand.CreateViaAutoReservation = createViaAutoReservation;
            }

            await _mediator.Send(cacheReservationEmployerCommand);

            routeModel.Id = cacheReservationEmployerCommand.Id;
        }
        catch (ReservationLimitReachedException)
        {
            return View("ReservationLimitReached", backUrl);
        }
        catch (MustCreateViaAutoReservationRouteException)
        {
            var continueRoute = _urlHelper.GenerateAddApprenticeUrl(null,
                routeModel.AccountLegalEntityPublicHashedId, "", viewModel.ProviderId, null,
                viewModel.CohortReference, routeModel.EmployerAccountId, string.IsNullOrEmpty(viewModel.CohortReference) && isEmployerSelect,
                "", viewModel.EncodedPledgeApplicationId, viewModel.JourneyData);

            return Redirect(continueRoute);
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
                return View("ProviderFundingPaused", backUrl);
            }

            return View("EmployerFundingPaused", backUrl);
        }

        var routeName = RouteNames.ProviderApprenticeshipTrainingRuleCheck;
        if (!routeModel.UkPrn.HasValue)
        {
            routeName = RouteNames.EmployerSelectCourseRuleCheck;
        }

        return RedirectToRoute(routeName, routeModel);
    }

    private bool IsThisAnEmployer()
    {
        return _configuration["AuthType"] != null &&
               _configuration["AuthType"].Equals("employer", StringComparison.CurrentCultureIgnoreCase);
    }

    private async Task<string> CheckCanAutoReserve(long accountId, string transferSenderId, string journeyData, string accountLegalEntityPublicHashedId, uint? ukPrn, string cohortRef, string hashedAccountId, Guid? userId, string encodedPledgeApplicationId)
    {
        var levyReservation = await _mediator.Send(new CreateReservationLevyEmployerCommand
        {
            AccountId = accountId,
            TransferSenderId = string.IsNullOrEmpty(transferSenderId) ? (long?)null : _encodingService.Decode(transferSenderId, EncodingType.PublicAccountId),
            TransferSenderEmployerAccountId = transferSenderId,
            UserId = userId,
            AccountLegalEntityId = _encodingService.Decode(
                accountLegalEntityPublicHashedId,
                EncodingType.PublicAccountLegalEntityId),
            EncodedPledgeApplicationId = encodedPledgeApplicationId
        });

        if (levyReservation != null)
        {
            var isEmployerSelect = IsThisAnEmployer();

            return _urlHelper.GenerateAddApprenticeUrl(levyReservation.ReservationId,
                accountLegalEntityPublicHashedId, "", ukPrn, null,
                cohortRef, hashedAccountId, string.IsNullOrEmpty(cohortRef) && isEmployerSelect,
                transferSenderId, encodedPledgeApplicationId, journeyData);
        }

        return string.Empty;
    }

    private async Task<CacheReservationEmployerCommand> BuildEmployerReservationCacheCommand(
        string employerAccountId, string accountLegalEntityPublicHashedId, string cohortRef,
        uint? providerId, string journeyData)
    {
        var accountId = _encodingService.Decode(employerAccountId, EncodingType.AccountId);
        var accountLegalEntity = await _mediator.Send(new GetLegalEntitiesQuery { AccountId = accountId });
        var legalEntity = accountLegalEntity.AccountLegalEntities.SingleOrDefault(c =>
            c.AccountLegalEntityPublicHashedId.Equals(accountLegalEntityPublicHashedId));

        if (legalEntity == null)
        {
            throw new AccountLegalEntityNotFoundException(accountLegalEntityPublicHashedId);
        }

        return new CacheReservationEmployerCommand
        {
            AccountLegalEntityName = legalEntity.AccountLegalEntityName,
            AccountLegalEntityPublicHashedId = accountLegalEntityPublicHashedId,
            AccountId = accountId,
            AccountLegalEntityId = legalEntity.AccountLegalEntityId,
            UkPrn = providerId,
            Id = Guid.NewGuid(),
            CohortRef = cohortRef,
            IsEmptyCohortFromSelect = string.IsNullOrEmpty(cohortRef),
            JourneyData = journeyData
        };
    }

    private long? GetCohortId(string cohortReference)
    {
        long? result = null;
        if (!string.IsNullOrEmpty(cohortReference))
        {
            result = _encodingService.Decode(cohortReference, EncodingType.CohortReference);
        }

        return result;
    }

    private string GetBackUrl(ReservationsRouteModel routeModel, SelectReservationViewModel viewModel)
    {
        if (_configuration["AuthType"] != null &&
            _configuration["AuthType"].Equals("provider", StringComparison.CurrentCultureIgnoreCase)
            && string.IsNullOrWhiteSpace(viewModel.CohortReference) && routeModel.UkPrn.HasValue)
        {
            return _urlHelper.GenerateConfirmEmployerUrl(routeModel.UkPrn.Value, routeModel.AccountLegalEntityPublicHashedId);
        }

        return _urlHelper.GenerateCohortDetailsUrl(routeModel.UkPrn, routeModel.EmployerAccountId,
            viewModel.CohortReference, journeyData: viewModel.JourneyData);
    }
}