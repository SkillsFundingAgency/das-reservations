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
using SFA.DAS.Reservations.Application.FundingRules.Queries.GetFundingRules;
using SFA.DAS.Reservations.Application.FundingRules.Queries.GetNextActiveGlobalFundingRule;
using SFA.DAS.Reservations.Application.Reservations.Commands.CacheReservationCourse;
using SFA.DAS.Reservations.Application.Reservations.Commands.CacheReservationEmployer;
using SFA.DAS.Reservations.Application.Reservations.Queries.GetCachedReservation;
using SFA.DAS.Reservations.Application.Reservations.Queries.GetCourses;
using SFA.DAS.Reservations.Domain.Rules;
using SFA.DAS.Reservations.Infrastructure.Configuration;
using SFA.DAS.Reservations.Infrastructure.Exceptions;
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
        private readonly ReservationsWebConfiguration _config;

        public EmployerReservationsController(IMediator mediator, IEncodingService encodingService, IOptions<ReservationsWebConfiguration> options)
        {
            _mediator = mediator;
            _encodingService = encodingService;
            _config = options.Value;
        }

        // GET
        public async Task<IActionResult> Index()
        {
            var response = await _mediator.Send(new GetNextActiveGlobalFundingRuleQuery());

            var nextGlobalRuleStartDate = response?.Rule?.ActiveFrom;

            if (!nextGlobalRuleStartDate.HasValue)
            {
                return RedirectToAction("Start", RouteData?.Values);
            }

            var viewModel = new FundingRestrictionNotificationViewModel
            {
                RestrictionStartDate = nextGlobalRuleStartDate.Value,
                BackLink = _config.EmployerDashboardUrl
            };

            return View("FundingRestrictionNotification", viewModel);
        }

        [HttpGet]
        [Route("start",Name = RouteNames.EmployerStart)]
        public async Task<IActionResult> Start(string employerAccountId)
        {
            try
            {
                var globalRules = await _mediator.Send(new GetFundingRulesQuery());

                if (!(globalRules?.ActiveGlobalRules.Any() ?? false))
                {
                    return View("Index");
                }

                var rule = globalRules.ActiveGlobalRules.First().RuleType;

                switch (rule)
                {
                    case GlobalRuleType.FundingPaused:
                        return View("EmployerFundingPaused");

                    case GlobalRuleType.ReservationLimit:
                        return View("ReservationLimitReached");
                    default:
                        return View("Index");
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

            var response = await _mediator.Send(new GetLegalEntitiesQuery { AccountId = _encodingService.Decode(routeModel.EmployerAccountId, EncodingType.AccountId), });
            var viewModel = new SelectLegalEntityViewModel(routeModel, response.AccountLegalEntities, cachedResponse?.AccountLegalEntityId);
            return View("SelectLegalEntity", viewModel);
        }

        [HttpPost]
        [Route("select-legal-entity/{id?}", Name = RouteNames.EmployerSelectLegalEntity)]
        public async Task<IActionResult> PostSelectLegalEntity(ReservationsRouteModel routeModel, ConfirmLegalEntityViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return await SelectLegalEntity(routeModel);
            }

            var response = await _mediator.Send(new GetLegalEntitiesQuery {AccountId = _encodingService.Decode(routeModel.EmployerAccountId, EncodingType.AccountId) });
            var selectedAccountLegalEntity = response.AccountLegalEntities.Single(model =>
                model.AccountLegalEntityPublicHashedId == viewModel.LegalEntity);
            var reservationId = routeModel.Id ?? Guid.NewGuid();

            try
            {
                await _mediator.Send(new CacheReservationEmployerCommand
                {
                    Id = reservationId,
                    AccountId = _encodingService.Decode(routeModel.EmployerAccountId, EncodingType.AccountId),
                    AccountLegalEntityId = selectedAccountLegalEntity.AccountLegalEntityId,
                    AccountLegalEntityName = selectedAccountLegalEntity.AccountLegalEntityName,
                    AccountLegalEntityPublicHashedId = selectedAccountLegalEntity.AccountLegalEntityPublicHashedId
                });

                routeModel.Id = reservationId;

                return RedirectToRoute(RouteNames.EmployerSelectCourse, routeModel);
            }
            catch (ValidationException e)
            {
                foreach (var member in e.ValidationResult.MemberNames)
                {
                    ModelState.AddModelError(member.Split('|')[0], member.Split('|')[1]);
                }

                return await SelectLegalEntity(routeModel);
            }
            catch (ReservationLimitReachedException)
            {
                return View("ReservationLimitReached");
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
                BackLink = routeModel.FromReview.HasValue && routeModel.FromReview.Value ? RouteNames.EmployerReview : RouteNames.EmployerSelectLegalEntity 
            };

            return View(viewModel);
        }

        [HttpGet]
        [Route("{id}/skip-course-selection",Name = RouteNames.EmployerSkipSelectCourse)]
        public async Task<IActionResult> SkipSelectCourse(ReservationsRouteModel routeModel)
        {
            return await PostSelectCourse(routeModel, null);
        }

        [HttpPost]
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
                    Courses = courseViewModels
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
