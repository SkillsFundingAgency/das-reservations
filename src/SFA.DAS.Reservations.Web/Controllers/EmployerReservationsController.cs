﻿using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SFA.DAS.Encoding;
using SFA.DAS.Reservations.Application.Employers.Queries.GetLegalEntities;
using SFA.DAS.Reservations.Application.Exceptions;
using SFA.DAS.Reservations.Application.FundingRules.Commands.MarkRuleAsRead;
using SFA.DAS.Reservations.Application.FundingRules.Queries.GetAccountFundingRules;
using SFA.DAS.Reservations.Application.FundingRules.Queries.GetNextUnreadGlobalFundingRule;
using SFA.DAS.Reservations.Application.Reservations.Commands.CacheReservationCourse;
using SFA.DAS.Reservations.Application.Reservations.Commands.CacheReservationEmployer;
using SFA.DAS.Reservations.Application.Reservations.Queries.GetCachedReservation;
using SFA.DAS.Reservations.Application.Reservations.Queries.GetCourses;
using SFA.DAS.Reservations.Domain.Employers;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Domain.Rules;
using SFA.DAS.Reservations.Domain.Rules.Api;
using SFA.DAS.Reservations.Infrastructure.Configuration;
using SFA.DAS.Reservations.Infrastructure.Exceptions;
using SFA.DAS.Reservations.Web.Filters;
using SFA.DAS.Reservations.Web.Infrastructure;
using SFA.DAS.Reservations.Web.Models;

namespace SFA.DAS.Reservations.Web.Controllers
{
    [Authorize(Policy = nameof(PolicyNames.HasEmployerAccount))]
    [Route("accounts/{employerAccountId}/reservations", Name = RouteNames.EmployerIndex)]
    public class EmployerReservationsController : Controller
    {
        private readonly IMediator _mediator;
        private readonly IEncodingService _encodingService;
        private readonly IExternalUrlHelper _urlHelper;
        private readonly ReservationsWebConfiguration _config;

        public EmployerReservationsController(IMediator mediator, IEncodingService encodingService, IOptions<ReservationsWebConfiguration> options, IExternalUrlHelper urlHelper)
        {
            _mediator = mediator;
            _encodingService = encodingService;
            _urlHelper = urlHelper;
            _config = options.Value;
        }

        // GET
        [ServiceFilter(typeof(NonEoiNotPermittedFilterAttribute))]
        public async Task<IActionResult> Index()
        {
            var userAccountIdClaim = User.Claims.First(c => c.Type.Equals(EmployerClaims.IdamsUserIdClaimTypeIdentifier));
            var response = await _mediator.Send(new GetNextUnreadGlobalFundingRuleQuery{Id = userAccountIdClaim.Value});

            var nextGlobalRuleId = response?.Rule?.Id;
            var nextGlobalRuleStartDate = response?.Rule?.ActiveFrom;

            if (!nextGlobalRuleId.HasValue || nextGlobalRuleId.Value == 0|| !nextGlobalRuleStartDate.HasValue)
            {
                return RedirectToAction("Start", RouteData?.Values);
            }

            var viewModel = new FundingRestrictionNotificationViewModel
            {
                RuleId = nextGlobalRuleId.Value,
                TypeOfRule = RuleType.GlobalRule,
                RestrictionStartDate = nextGlobalRuleStartDate.Value,
                BackLink = _config.EmployerDashboardUrl
            };

            return View("FundingRestrictionNotification", viewModel);
        }
            

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("saveRuleNotificationChoice",Name = RouteNames.EmployerSaveRuleNotificationChoice)]
        public async Task<IActionResult> SaveRuleNotificationChoice(long ruleId, RuleType typeOfRule, bool markRuleAsRead)
        {
            if(!markRuleAsRead)
            {
                return RedirectToRoute(RouteNames.EmployerStart);
            }

            var userAccountIdClaim = HttpContext.User.Claims.First(c => c.Type.Equals(EmployerClaims.IdamsUserIdClaimTypeIdentifier));
			
            var userId = userAccountIdClaim.Value;

            var command = new MarkRuleAsReadCommand
            {
                Id = userId,
                RuleId = ruleId,
                TypeOfRule = typeOfRule
            };
            
            await _mediator.Send(command);

            return RedirectToRoute(RouteNames.EmployerStart);
        }


        [HttpGet]
        [Route("start",Name = RouteNames.EmployerStart)]
        public async Task<IActionResult> Start(ReservationsRouteModel routeModel)
        {
            try
            {
                var viewModel = new EmployerStartViewModel
                {
                    FindApprenticeshipTrainingUrl = _config.FindApprenticeshipTrainingUrl,
                    ApprenticeshipFundingRulesUrl = _config.ApprenticeshipFundingRulesUrl
                };

                var accountId = _encodingService.Decode(routeModel.EmployerAccountId, EncodingType.AccountId);

                var response = await _mediator.Send(new GetAccountFundingRulesQuery{ AccountId = accountId});
                var activeGlobalRuleType = response?.ActiveRule;

	            if (activeGlobalRuleType == null)
	            {
	                return View("Index", viewModel);
	            }

                switch (activeGlobalRuleType)
                {
                    case GlobalRuleType.FundingPaused:
                        return View("EmployerFundingPaused");

                    case GlobalRuleType.ReservationLimit:
                        return View("ReservationLimitReached", GenerateLimitReachedBackLink(routeModel));
                    default:
                        return View("Index", viewModel);
                }
            }
            catch (ValidationException)
            {
                return View("Error");
            }
        }
            

        [HttpGet]
        [Route("select-legal-entity/{id?}", Name = RouteNames.EmployerSelectLegalEntity)]
        public async Task<IActionResult> SelectLegalEntity(ReservationsRouteModel routeModel)
        {
            GetCachedReservationResult cachedResponse = null;
            if (routeModel.Id.HasValue)
            {
                cachedResponse = await _mediator.Send(new GetCachedReservationQuery {Id = routeModel.Id.Value});
            }

            var legalEntitiesResponse = await _mediator.Send(new GetLegalEntitiesQuery { AccountId = _encodingService.Decode(routeModel.EmployerAccountId, EncodingType.AccountId) });

            if (legalEntitiesResponse.AccountLegalEntities.Count() == 1)
            {
                var accountLegalEntity = legalEntitiesResponse.AccountLegalEntities.First();
                await CacheReservation(routeModel, accountLegalEntity);
                return RedirectToRoute(RouteNames.EmployerSelectCourse, routeModel);
            }

            var viewModel = new SelectLegalEntityViewModel(routeModel, legalEntitiesResponse.AccountLegalEntities, cachedResponse?.AccountLegalEntityPublicHashedId);
            return View("SelectLegalEntity", viewModel);
        }

        private async Task CacheReservation(ReservationsRouteModel routeModel, AccountLegalEntity accountLegalEntity)
        {
            var reservationId = routeModel.Id ?? Guid.NewGuid();
            
            await _mediator.Send(new CacheReservationEmployerCommand
            {
                Id = reservationId,
                AccountId = accountLegalEntity.AccountId,
                AccountLegalEntityId = accountLegalEntity.AccountLegalEntityId,
                AccountLegalEntityName = accountLegalEntity.AccountLegalEntityName,
                AccountLegalEntityPublicHashedId = accountLegalEntity.AccountLegalEntityPublicHashedId
            });

            routeModel.Id = reservationId;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("select-legal-entity/{id?}", Name = RouteNames.EmployerSelectLegalEntity)]
        public async Task<IActionResult> PostSelectLegalEntity(ReservationsRouteModel routeModel, ConfirmLegalEntityViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                var legalEntitiesResponse = await _mediator.Send(new GetLegalEntitiesQuery { AccountId = _encodingService.Decode(routeModel.EmployerAccountId, EncodingType.AccountId) });
                var requestViewModel = new SelectLegalEntityViewModel(routeModel, legalEntitiesResponse.AccountLegalEntities, viewModel.LegalEntity);
                return View("SelectLegalEntity", requestViewModel);
            }

            try
            {
                var decodedAccountId = _encodingService.Decode(routeModel.EmployerAccountId, EncodingType.AccountId);
                var response = await _mediator.Send(new GetLegalEntitiesQuery {AccountId = decodedAccountId });
                var selectedAccountLegalEntity = response.AccountLegalEntities.Single(model =>
                    model.AccountLegalEntityPublicHashedId == viewModel.LegalEntity);

                await CacheReservation(routeModel, selectedAccountLegalEntity);

                return RedirectToRoute(RouteNames.EmployerSelectCourse, routeModel);
            }
            catch (ValidationException e)
            {
                foreach (var member in e.ValidationResult.MemberNames)
                {
                    ModelState.AddModelError(member.Split('|')[0], member.Split('|')[1]);
                }

                var legalEntitiesResponse = await _mediator.Send(new GetLegalEntitiesQuery { AccountId = _encodingService.Decode(routeModel.EmployerAccountId, EncodingType.AccountId) });
                var requestViewModel = new SelectLegalEntityViewModel(routeModel, legalEntitiesResponse.AccountLegalEntities, viewModel.LegalEntity);
                return View("SelectLegalEntity", requestViewModel);
            }
            catch (ReservationLimitReachedException)
            {
                return View("ReservationLimitReached", GenerateLimitReachedBackLink(routeModel));
            }
        }

        [HttpGet]
        [Route("{id}/select-course",Name = RouteNames.EmployerSelectCourse)]
        public async Task<IActionResult> SelectCourse(ReservationsRouteModel routeModel)
        {
            var cachedReservation = await _mediator.Send(new GetCachedReservationQuery {Id = routeModel.Id.Value});
            if (cachedReservation == null)
            {
                return View("Index");
            }

            var getCoursesResponse = await _mediator.Send(new GetCoursesQuery());

            var courseViewModels = getCoursesResponse.Courses.Select(course => new CourseViewModel(course, cachedReservation.CourseId));

            var viewModel = new EmployerSelectCourseViewModel
            {
                ReservationId = routeModel.Id.Value,
                Courses = courseViewModels,
                BackLink = GenerateBackLink(routeModel, cachedReservation.CohortRef),
                CohortReference = cachedReservation.CohortRef
            };

            return View(viewModel);
        }

        private string GenerateBackLink(ReservationsRouteModel routeModel, string cohortRef)
        {
            if (!string.IsNullOrEmpty(routeModel.CohortReference))
            {
                return _urlHelper.GenerateCohortDetailsUrl(null, routeModel.EmployerAccountId, cohortRef);
            }
            return routeModel.FromReview.HasValue && routeModel.FromReview.Value ? RouteNames.EmployerReview : RouteNames.EmployerSelectLegalEntity;
        }

        private string GenerateLimitReachedBackLink(ReservationsRouteModel routeModel)
        {
            if (!string.IsNullOrEmpty(routeModel.CohortReference))
            {
                return _urlHelper.GenerateCohortDetailsUrl(null, routeModel.EmployerAccountId, routeModel.CohortReference);
            }

            return Url.RouteUrl(RouteNames.EmployerManage, routeModel);
        }
        

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("{id}/select-course", Name = RouteNames.EmployerSelectCourse)]
        public async Task<IActionResult> PostSelectCourse(ReservationsRouteModel routeModel, string selectedCourseId)
        {
            try
            {
                await _mediator.Send(new CacheReservationCourseCommand
                {
                    Id = routeModel.Id.Value,
                    CourseId = selectedCourseId

                });

                return RedirectToRoute(RouteNames.EmployerApprenticeshipTraining, new ReservationsRouteModel
                {
                    Id = routeModel.Id,
                    EmployerAccountId = routeModel.EmployerAccountId,
                    CohortReference = routeModel.CohortReference
                });
            }
            catch (ValidationException e)
            {
                foreach (var member in e.ValidationResult.MemberNames)
                {
                    ModelState.AddModelError(member.Split('|')[0], member.Split('|')[1]);
                }

                var getCoursesResponse = await _mediator.Send(new GetCoursesQuery());

                if (getCoursesResponse?.Courses == null)
                {
                    return View("Error"); //todo: setup view correctly.
                }

                var courseViewModels = getCoursesResponse.Courses.Select(c => new CourseViewModel(c));

                var viewModel = new EmployerSelectCourseViewModel
                {
                    ReservationId = routeModel.Id.Value,
                    Courses = courseViewModels,
                    BackLink = GenerateBackLink(routeModel, routeModel.CohortReference),
                    CohortReference = routeModel.CohortReference
                };

                return View("SelectCourse", viewModel);
            }
            catch (CachedReservationNotFoundException)
            {
                throw new ArgumentException("Reservation not found", nameof(routeModel.Id));
            }
        }
    }
}
