using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Internal;
using SFA.DAS.Reservations.Application.Employers.Queries;
using SFA.DAS.Reservations.Application.FundingRules.Queries.GetFundingRules;
using SFA.DAS.Reservations.Application.Reservations.Commands.CacheReservationEmployer;
using SFA.DAS.Reservations.Application.Reservations.Queries.GetCachedReservation;
using SFA.DAS.Reservations.Web.Infrastructure;
using SFA.DAS.Reservations.Web.Models;

namespace SFA.DAS.Reservations.Web.Controllers
{
    [Authorize(Policy = nameof(PolicyNames.HasProviderAccount))]
    [Route("{ukPrn}/reservations", Name = RouteNames.ProviderIndex)]
    public class ProviderReservationsController : Controller
    {
        private readonly IMediator _mediator;

        public ProviderReservationsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<IActionResult> Index()
        {
            var response = await _mediator.Send(new GetFundingRulesQuery());

            if (response?.FundingRules?.GlobalRules != null && response.FundingRules.GlobalRules.Any())
            {
                return View( "fundingStopped");
            }

            return View("index");
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
        [Route("confirm-employer", Name=RouteNames.ProviderConfirmEmployer)]
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
                
                return View(viewModel);

            }

            return View(viewModel);
        }

        [HttpPost]
        [Route("confirm-employer", Name = RouteNames.ProviderConfirmEmployer)]
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
                    UkPrn = viewModel.UkPrn
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
        }
    }
}