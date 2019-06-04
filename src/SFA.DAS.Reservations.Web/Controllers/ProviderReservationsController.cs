using System;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SFA.DAS.Reservations.Application.Employers.Queries;
using SFA.DAS.Reservations.Application.Exceptions;
using SFA.DAS.Reservations.Application.FundingRules.Commands.MarkRuleAsRead;
using SFA.DAS.Reservations.Application.FundingRules.Queries.GetFundingRules;
using SFA.DAS.Reservations.Application.FundingRules.Queries.GetNextUnreadGlobalFundingRule;
using SFA.DAS.Reservations.Application.Reservations.Commands.CacheReservationEmployer;
using SFA.DAS.Reservations.Application.Reservations.Queries.GetCachedReservation;
using SFA.DAS.Reservations.Domain.Rules.Api;
using SFA.DAS.Reservations.Infrastructure.Configuration;
using SFA.DAS.Reservations.Web.Infrastructure;
using SFA.DAS.Reservations.Web.Models;

namespace SFA.DAS.Reservations.Web.Controllers
{
    [Authorize(Policy = nameof(PolicyNames.HasProviderAccount))]
    [Route("{ukPrn}/reservations", Name = RouteNames.ProviderIndex)]
    public class ProviderReservationsController : Controller
    {
        private readonly IMediator _mediator;
        private readonly ReservationsWebConfiguration _config;

        public ProviderReservationsController(IMediator mediator, IOptions<ReservationsWebConfiguration> options)
        {
            _mediator = mediator;
            _config = options.Value;
        }

        public async Task<IActionResult> Index(uint ukPrn)
        {
            var response = await _mediator.Send(new GetFundingRulesQuery());

            if (response?.ActiveGlobalRules != null && response.ActiveGlobalRules.Any())
            {
                return View( "ProviderFundingPaused");
            }

            var employers = (await _mediator.Send(new GetTrustedEmployersQuery { UkPrn = ukPrn })).Employers.ToList();

            if (!employers.Any())
            {
                return View("NoPermissions");
            }
            
            return View("Index");
        }

        [HttpGet]
        [Route("choose-employer", Name = RouteNames.ProviderChooseEmployer)]
        public async Task<IActionResult> ChooseEmployer(ReservationsRouteModel routeModel)
        {
            if (!routeModel.UkPrn.HasValue)
            {
                throw new ArgumentException("UkPrn must be set", nameof(ReservationsRouteModel.UkPrn));
            }

            var employers = await _mediator.Send(new GetTrustedEmployersQuery {UkPrn = routeModel.UkPrn.Value});

            var viewModel = new ChooseEmployerViewModel
            {
                Employers = employers.Employers
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
