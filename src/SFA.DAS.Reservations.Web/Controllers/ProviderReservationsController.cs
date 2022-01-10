using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Encoding;
using SFA.DAS.Reservations.Application.Exceptions;
using SFA.DAS.Reservations.Application.FundingRules.Queries.GetFundingRules;
using SFA.DAS.Reservations.Application.Providers.Queries.GetLegalEntityAccount;
using SFA.DAS.Reservations.Application.Providers.Queries.GetTrustedEmployers;
using SFA.DAS.Reservations.Application.Reservations.Commands.CacheReservationEmployer;
using SFA.DAS.Reservations.Application.Reservations.Queries.GetCachedReservation;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Domain.Rules;
using SFA.DAS.Reservations.Web.Extensions;
using SFA.DAS.Reservations.Web.Infrastructure;
using SFA.DAS.Reservations.Web.Models;

namespace SFA.DAS.Reservations.Web.Controllers
{
    [Authorize(Policy = nameof(PolicyNames.HasProviderAccount))]
    [Authorize(Policy = nameof(PolicyNames.HasProviderGotContributorOrHigherRoleOrIsEmployer))]
    [Route("{ukPrn}/reservations", Name = RouteNames.ProviderIndex)]
    public class ProviderReservationsController : ReservationsBaseController
    {
        private readonly IMediator _mediator;
        private readonly IExternalUrlHelper _externalUrlHelper;
        private readonly IEncodingService _encodingService;
        private readonly ISessionStorageService<GetTrustedEmployersResponse> _sessionStorageService;

        public ProviderReservationsController(
            IMediator mediator, 
            IExternalUrlHelper externalUrlHelper, 
            IEncodingService encodingService,
            ISessionStorageService<GetTrustedEmployersResponse> sessionStorageService) : base(mediator)
        {
            _mediator = mediator;
            _externalUrlHelper = externalUrlHelper;
            _encodingService = encodingService;
            _sessionStorageService = sessionStorageService;
        }

        public async Task<IActionResult> Index(ReservationsRouteModel routeModel)
        {
            var backLink = routeModel.IsFromManage.HasValue && routeModel.IsFromManage.Value
                ? Url.RouteUrl(RouteNames.ProviderManage,routeModel)
                : _externalUrlHelper.GenerateDashboardUrl();

            var viewResult = await CheckNextGlobalRule(RouteNames.ProviderStart, ProviderClaims.ProviderUkprn, backLink, RouteNames.ProviderSaveRuleNotificationChoiceNoReservation);
            
            if (viewResult == null)
            {
                return RedirectToRoute(RouteNames.ProviderStart, routeModel);
            }

            return viewResult;
        }

        [Route("start", Name = RouteNames.ProviderStart)]
        public async Task<IActionResult> Start(uint ukPrn, bool isFromManage)
        {
            var response = await _mediator.Send(new GetFundingRulesQuery());

            var activeGlobalRule = response?.ActiveGlobalRules?.OrderBy(x => x.ActiveFrom).FirstOrDefault();

            var viewModel = new ProviderStartViewModel
            {
                IsFromManage = isFromManage
            };

            if (activeGlobalRule != null)
            {
                var backLink = isFromManage
                    ? Url.RouteUrl(RouteNames.ProviderManage, new {ukPrn, isFromManage})
                    : _externalUrlHelper.GenerateDashboardUrl(); 

                switch(activeGlobalRule.RuleType)
                {
                    case GlobalRuleType.FundingPaused: 
                        return View("ProviderFundingPaused", backLink);

                    case GlobalRuleType.DynamicPause:
                        viewModel.ActiveGlobalRule = new GlobalRuleViewModel(activeGlobalRule);
                        break;
                        // TODO: Set globalrule same as employer side
                }               
            }

            var employers = (await _mediator.Send(new GetTrustedEmployersQuery { UkPrn = ukPrn })).Employers.ToList();

            if (!employers.Any())
            {
                return View("NoPermissions");
            }

            return View("Index", viewModel);
        }

        [HttpGet]
        [Route("choose-employer", Name = RouteNames.ProviderChooseEmployer)]
        public async Task<IActionResult> ChooseEmployer(ReservationsRouteModel routeModel)
        {
            if (!routeModel.UkPrn.HasValue)
            {
                throw new ArgumentException("UkPrn must be set", nameof(ReservationsRouteModel.UkPrn));
            }

            var getTrustedEmployersResponse = _sessionStorageService.Get(); 

            if (getTrustedEmployersResponse == null)
            {
                getTrustedEmployersResponse = await _mediator.Send(new GetTrustedEmployersQuery {UkPrn = routeModel.UkPrn.Value});
                _sessionStorageService.Store(getTrustedEmployersResponse);
            }

            var sortModel = new SortModel
            {
                ReverseSort = routeModel.ReverseSort,
                SortField = routeModel.SortField
            };

            var eoiEmployers = getTrustedEmployersResponse.Employers
                .Where(e => string.IsNullOrWhiteSpace(routeModel.SearchTerm) ||
                            e.AccountName.Replace(" ", string.Empty).Contains(routeModel.SearchTerm.Replace(" ", string.Empty), StringComparison.CurrentCultureIgnoreCase) ||
                            e.AccountLegalEntityName.Replace(" ", string.Empty).Contains(routeModel.SearchTerm.Replace(" ", string.Empty), StringComparison.CurrentCultureIgnoreCase))
                .ToList();


            eoiEmployers = eoiEmployers.Order(sortModel);

            var viewModel = new ChooseEmployerViewModel
            {
                Employers = eoiEmployers,
                SearchTerm = routeModel.SearchTerm,
                SortModel = sortModel
            };

            return View(viewModel);
        }

        [HttpGet]
        [Route("choose-employer-search", Name = RouteNames.ProviderChooseEmployerSearch)]
        public async Task<JsonResult> ChooseEmployerSearch(ReservationsRouteModel routeModel, string searchTerm)
        {
            if (!routeModel.UkPrn.HasValue)
            {
                throw new ArgumentException("UkPrn must be set", nameof(ReservationsRouteModel.UkPrn));
            }

            var getTrustedEmployersResponse = _sessionStorageService.Get();

            var eoiEmployersAccount = getTrustedEmployersResponse.Employers
                .Where(eoi => eoi.AccountName.Contains(searchTerm, StringComparison.CurrentCultureIgnoreCase))
                .Select(eoi => eoi.AccountName.ToUpper());

            var eoiEmployersEmployer = getTrustedEmployersResponse.Employers
                .Where(eoi => eoi.AccountLegalEntityName.Contains(searchTerm, StringComparison.CurrentCultureIgnoreCase))
                .Select(eoi => eoi.AccountLegalEntityName.ToUpper());

            return Json(eoiEmployersAccount.Concat(eoiEmployersEmployer).Distinct().OrderBy(eoi => eoi));
        }

        [HttpGet]
        [Route("confirm-employer/{id?}", Name=RouteNames.ProviderConfirmEmployer)]
        public async Task<IActionResult> ConfirmEmployer(ConfirmEmployerViewModel viewModel)
        {
            _sessionStorageService.Delete();

            if (viewModel.Id.HasValue)
            {
                var result = await _mediator.Send(new GetCachedReservationQuery
                {
                    Id = viewModel.Id.Value,
                    UkPrn = viewModel.UkPrn
                });

                viewModel.AccountLegalEntityName = result.AccountLegalEntityName;
                viewModel.AccountPublicHashedId = _encodingService.Encode(result.AccountId, EncodingType.AccountId);
                viewModel.AccountLegalEntityPublicHashedId = result.AccountLegalEntityPublicHashedId;
                viewModel.AccountName = result.AccountName;
                return View(viewModel);
            }

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("confirm-employer/{id?}", Name = RouteNames.ProviderConfirmEmployer)]
        public async Task<IActionResult> ProcessConfirmEmployer(ConfirmEmployerViewModel viewModel)
        {
            if (!viewModel.Confirm.HasValue)
            {
                ModelState.AddModelError("confirm-yes", "Select whether to secure funds for this employer or not");
                return View("ConfirmEmployer", viewModel);
            }

            try
            {
                if (!viewModel.Confirm.Value)
                {
                    return RedirectToRoute(RouteNames.ProviderChooseEmployer, new
                    {
                        viewModel.UkPrn
                    });
                }

                var reservationId = Guid.NewGuid();

                await _mediator.Send(new CacheReservationEmployerCommand
                {
                    Id = reservationId,
                    AccountId = _encodingService.Decode(viewModel.AccountPublicHashedId, EncodingType.PublicAccountId),
                    AccountLegalEntityId = _encodingService.Decode(viewModel.AccountLegalEntityPublicHashedId, EncodingType.PublicAccountLegalEntityId),
                    AccountLegalEntityName = viewModel.AccountLegalEntityName,
                    AccountLegalEntityPublicHashedId = viewModel.AccountLegalEntityPublicHashedId,
                    UkPrn = viewModel.UkPrn,
                    AccountName = viewModel.AccountName
                });

                return RedirectToRoute(RouteNames.ProviderApprenticeshipTraining, new
                {
                    Id = reservationId,
                    PublicHashedEmployerAccountId = viewModel.AccountPublicHashedId,
                    viewModel.UkPrn
                });

            }
            catch (ValidationException e)
            {
                foreach (var member in e.ValidationResult.MemberNames)
                {
                    ModelState.AddModelError(member.Split('|')[0], member.Split('|')[1]);
                }

                return View("ConfirmEmployer", viewModel);
            }
            catch (ReservationLimitReachedException)
            {
                return View("ReservationLimitReached");
            }
        }

        [HttpGet]
        [Route("employer-agreement-not-signed/{id?}", Name=RouteNames.ProviderEmployerAgreementNotSigned)]
        public async Task<IActionResult> EmployerAgreementNotSigned(ReservationsRouteModel routeModel, string id)
        {

            if (string.IsNullOrEmpty(id))
            {
                id = routeModel.AccountLegalEntityPublicHashedId;
            }
            
            var result = await _mediator.Send(new GetAccountLegalEntityQuery {AccountLegalEntityPublicHashedId = id});
            var viewModel = new EmployerAgreementNotSignedViewModel
            {
                AccountName = result.LegalEntity.AccountLegalEntityName, 
                DashboardUrl = _externalUrlHelper.GenerateDashboardUrl(null),
                BackUrl = routeModel.IsFromSelect.HasValue && routeModel.IsFromSelect.Value ? routeModel.PreviousPage : "",
                IsUrl = routeModel.IsFromSelect.HasValue && routeModel.IsFromSelect.Value
            };

            return View(viewModel);
        }
    }
}
