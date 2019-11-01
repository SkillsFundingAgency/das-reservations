using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Common.Domain.Types;
using SFA.DAS.Reservations.Application.Employers.Queries;
using SFA.DAS.Reservations.Application.Employers.Queries.GetLegalEntities;
using SFA.DAS.Reservations.Application.Exceptions;
using SFA.DAS.Reservations.Application.FundingRules.Queries.GetFundingRules;
using SFA.DAS.Reservations.Application.Reservations.Commands.CacheReservationEmployer;
using SFA.DAS.Reservations.Application.Reservations.Queries.GetCachedReservation;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Web.Infrastructure;
using SFA.DAS.Reservations.Web.Models;

namespace SFA.DAS.Reservations.Web.Controllers
{
    [Authorize(Policy = nameof(PolicyNames.HasProviderAccount))]
    [Route("{ukPrn}/reservations", Name = RouteNames.ProviderIndex)]
    public class ProviderReservationsController : ReservationsBaseController
    {
        private readonly IMediator _mediator;
        private readonly IExternalUrlHelper _externalUrlHelper;

        public ProviderReservationsController(IMediator mediator, IExternalUrlHelper externalUrlHelper) : base(mediator)
        {
            _mediator = mediator;
            _externalUrlHelper = externalUrlHelper;
        }

        public async Task<IActionResult> Index(bool isFromManage)
        {
            var viewResult = await CheckNextGlobalRule(RouteNames.ProviderStart, ProviderClaims.ProviderUkprn, _externalUrlHelper.GenerateDashboardUrl());
            
            if (viewResult == null)
            {
                RouteData.Values.Add(nameof(isFromManage), isFromManage);
                return RedirectToRoute(RouteNames.ProviderStart, RouteData?.Values);
            }

            return viewResult;
        }

        [Route("start", Name = RouteNames.ProviderStart)]
        public async Task<IActionResult> Start(uint ukPrn, bool isFromManage)
        {
            var response = await _mediator.Send(new GetFundingRulesQuery());

            if (response?.ActiveGlobalRules != null && response.ActiveGlobalRules.Any())
            {
                var backLink = isFromManage
                    ? Url.RouteUrl(RouteNames.ProviderManage, new {ukPrn, isFromManage})
                    : _externalUrlHelper.GenerateDashboardUrl(); 
                return View( "ProviderFundingPaused", backLink);
            }

            var employers = (await _mediator.Send(new GetTrustedEmployersQuery { UkPrn = ukPrn })).Employers.ToList();

            if (!employers.Any())
            {
                return View("NoPermissions");
            }
            
            var viewModel = new ProviderStartViewModel
            {
                IsFromManage = isFromManage
            };
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

            var getTrustedEmployersResponse = await _mediator.Send(new GetTrustedEmployersQuery {UkPrn = routeModel.UkPrn.Value});
            
            // eoi filter
            var eoiEmployers = new List<Domain.Employers.Employer>();
            foreach (var employer in getTrustedEmployersResponse.Employers)
            {
                var getLegalEntitiesResponse = await _mediator.Send(new GetLegalEntitiesQuery{AccountId = employer.AccountId});
                if (getLegalEntitiesResponse.AccountLegalEntities.All(entity =>
                    !entity.IsLevy
                    && entity.AgreementType == AgreementType.NonLevyExpressionOfInterest))
                {
                    eoiEmployers.Add(employer);
                }
            }

            var viewModel = new ChooseEmployerViewModel
            {
                Employers = eoiEmployers
            };

            return View(viewModel);
        }

        [HttpGet]
        [Route("confirm-employer/{id?}", Name=RouteNames.ProviderConfirmEmployer)]
        public async Task<IActionResult> ConfirmEmployer(ConfirmEmployerViewModel viewModel)
        {

            if (viewModel.Id.HasValue)
            {
                var result = await _mediator.Send(new GetCachedReservationQuery
                {
                    Id = viewModel.Id.Value,
                    UkPrn = viewModel.UkPrn
                });

                viewModel.AccountLegalEntityName = result.AccountLegalEntityName;
                viewModel.AccountId = result.AccountId;
                viewModel.AccountLegalEntityId = result.AccountLegalEntityId;
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
                    return RedirectToAction("ChooseEmployer", "ProviderReservations", new
                    {
                        UkPrn = viewModel.UkPrn
                    });
                }

                var reservationId = Guid.NewGuid();

                await _mediator.Send(new CacheReservationEmployerCommand
                {
                    Id = reservationId,
                    AccountId = viewModel.AccountId,
                    AccountLegalEntityId = viewModel.AccountLegalEntityId,
                    AccountLegalEntityName = viewModel.AccountLegalEntityName,
                    AccountLegalEntityPublicHashedId = viewModel.AccountLegalEntityPublicHashedId,
                    UkPrn = viewModel.UkPrn,
                    AccountName = viewModel.AccountName
                });

                return RedirectToRoute(RouteNames.ProviderApprenticeshipTraining, new
                {
                    Id = reservationId,
                    EmployerAccountId = viewModel.AccountPublicHashedId,
                    UkPrn = viewModel.UkPrn
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
    }
}
