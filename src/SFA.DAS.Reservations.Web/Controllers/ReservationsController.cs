using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SFA.DAS.Encoding;
using SFA.DAS.Reservations.Application.FundingRules.Commands.MarkRuleAsRead;
using SFA.DAS.Reservations.Application.FundingRules.Queries.GetAccountFundingRules;
using SFA.DAS.Reservations.Application.FundingRules.Queries.GetFundingRules;
using SFA.DAS.Reservations.Application.Reservations.Commands.CacheReservationCourse;
using SFA.DAS.Reservations.Application.Reservations.Commands.CacheReservationStartDate;
using SFA.DAS.Reservations.Application.Reservations.Commands.CreateReservation;
using SFA.DAS.Reservations.Application.Reservations.Queries.GetCachedReservation;
using SFA.DAS.Reservations.Application.Reservations.Queries.GetCourses;
using SFA.DAS.Reservations.Application.Reservations.Queries.GetReservation;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Domain.Rules;
using SFA.DAS.Reservations.Infrastructure.Configuration;
using SFA.DAS.Reservations.Infrastructure.Exceptions;
using SFA.DAS.Reservations.Web.Extensions;
using SFA.DAS.Reservations.Web.Filters;
using SFA.DAS.Reservations.Web.Infrastructure;
using SFA.DAS.Reservations.Web.Models;
using SFA.DAS.Reservations.Web.Services;

namespace SFA.DAS.Reservations.Web.Controllers
{
    [Authorize(Policy = nameof(PolicyNames.HasProviderOrEmployerAccount))]
    [Authorize(Policy = nameof(PolicyNames.HasProviderGotContributorOrHigherRoleOrIsEmployer))]
    [ServiceFilter(typeof(LevyNotPermittedFilter))]
    public class ReservationsController : ReservationsBaseController
    {
        private readonly IMediator _mediator;
        private readonly ITrainingDateService _trainingDateService;
        private readonly ILogger<ReservationsController> _logger;
        private readonly IEncodingService _encodingService;
        private readonly ReservationsWebConfiguration _configuration;
        private readonly IExternalUrlHelper _urlHelper;

        public ReservationsController(
            IMediator mediator,
            ITrainingDateService trainingDateService,
            IOptions<ReservationsWebConfiguration> configuration,
            ILogger<ReservationsController> logger,
            IEncodingService encodingService,
            IExternalUrlHelper urlHelper) : base(mediator)
        {
            _mediator = mediator;
            _trainingDateService = trainingDateService;
            _logger = logger;
            _encodingService = encodingService;
            _configuration = configuration.Value;
            _urlHelper = urlHelper;
        }



        [HttpGet]
        [Route("accounts/{employerAccountId}/reservations/{id}/select-course-rule-check", Name = RouteNames.EmployerSelectCourseRuleCheck)]
        [Route("{ukPrn}/reservations/{id}/select-course-rule-check", Name = RouteNames.ProviderApprenticeshipTrainingRuleCheck)]
        public async Task<IActionResult> SelectCourseRuleCheck(ReservationsRouteModel routeModel)
        {
            //only comes from select
            var isProvider = routeModel.UkPrn != null;
            var redirectRouteName = isProvider ? RouteNames.ProviderApprenticeshipTraining : RouteNames.EmployerSelectCourse;
            var backLink = _urlHelper.GenerateCohortDetailsUrl(routeModel.UkPrn, routeModel.EmployerAccountId,
                routeModel.CohortReference, string.IsNullOrEmpty(routeModel.CohortReference));
            var identifier = isProvider ? ProviderClaims.ProviderUkprn : EmployerClaims.IdamsUserIdClaimTypeIdentifier;
            var postRouteName = isProvider
                ? RouteNames.ProviderSaveRuleNotificationChoice
                : RouteNames.EmployerSaveRuleNotificationChoice;


            var viewResult = await CheckNextGlobalRule(redirectRouteName, identifier, backLink, postRouteName);
            if (viewResult != null)
            {
                return viewResult;
            }

            return RedirectToRoute(redirectRouteName, routeModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("accounts/{employerAccountId}/reservations/saveRuleNotificationChoice", Name = RouteNames.EmployerSaveRuleNotificationChoiceNoReservation)]
        [Route("accounts/{employerAccountId}/reservations/{id}/saveRuleNotificationChoice", Name = RouteNames.EmployerSaveRuleNotificationChoice)]
        [Route("{ukPrn}/reservations/saveRuleNotificationChoice", Name = RouteNames.ProviderSaveRuleNotificationChoiceNoReservation)]
        [Route("{ukPrn}/reservations/{id}/saveRuleNotificationChoice", Name = RouteNames.ProviderSaveRuleNotificationChoice)]
        public async Task<IActionResult> SaveRuleNotificationChoice(ReservationsRouteModel routeModel, FundingRestrictionNotificationViewModel viewModel)
        {
            if (!viewModel.MarkRuleAsRead)
            {
                return RedirectToRoute(viewModel.RouteName);
            }

            var claim = routeModel.UkPrn != null ?
                HttpContext.User.Claims.First(c => c.Type.Equals(ProviderClaims.ProviderUkprn)) :
                HttpContext.User.Claims.First(c => c.Type.Equals(EmployerClaims.IdamsUserIdClaimTypeIdentifier));

            var claimValue = claim.Value;

            var command = new MarkRuleAsReadCommand
            {
                Id = claimValue,
                RuleId = viewModel.RuleId,
                TypeOfRule = viewModel.TypeOfRule
            };

            await _mediator.Send(command);

            return RedirectToRoute(viewModel.RouteName);
        }

        [Route("{ukPrn}/reservations/{id}/apprenticeship-training/{fromReview?}", Name = RouteNames.ProviderApprenticeshipTraining)]
        [Route("accounts/{employerAccountId}/reservations/{id}/apprenticeship-training", Name = RouteNames.EmployerApprenticeshipTraining)]
        public async Task<IActionResult> ApprenticeshipTraining(ReservationsRouteModel routeModel)
        {
            GetCachedReservationResult cachedReservation = null;

            if (routeModel.Id.HasValue)
            {
                cachedReservation = await _mediator.Send(new GetCachedReservationQuery { Id = routeModel.Id.GetValueOrDefault() });
                //todo: error handling if fails validation e.g. id not found, redirect to index.
            }

            var viewModel = await BuildApprenticeshipTrainingViewModel(
                routeModel.UkPrn != null,
                cachedReservation?.AccountLegalEntityPublicHashedId,
                cachedReservation?.AccountId ?? _encodingService.Decode(routeModel.EmployerAccountId, EncodingType.AccountId),
                cachedReservation?.CourseId,
                cachedReservation?.TrainingDate,
                routeModel.FromReview ?? false,
                cachedReservation?.CohortRef,
                routeModel.UkPrn);

            return View(viewModel);
        }

        [Route("{ukPrn}/reservations/{id}/apprenticeship-training", Name = RouteNames.ProviderCreateApprenticeshipTraining)]
        [Route("accounts/{employerAccountId}/reservations/{id}/apprenticeship-training", Name = RouteNames.EmployerCreateApprenticeshipTraining)]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> PostApprenticeshipTraining(ReservationsRouteModel routeModel, ApprenticeshipTrainingFormModel formModel)
        {
            var isProvider = routeModel.UkPrn != null;
            TrainingDateModel trainingDateModel = null;
            var employerAccountId = _encodingService.Decode(routeModel.EmployerAccountId, EncodingType.AccountId);

            try
            {
                if (!string.IsNullOrWhiteSpace(formModel.StartDate))
                    trainingDateModel = JsonConvert.DeserializeObject<TrainingDateModel>(formModel.StartDate);

                if (!ModelState.IsValid)
                {
                    var model = await BuildApprenticeshipTrainingViewModel(
                        isProvider,
                        formModel.AccountLegalEntityPublicHashedId,
                        employerAccountId,
                        formModel.SelectedCourseId,
                        trainingDateModel,
                        formModel.FromReview,
                        formModel.CohortRef,
                        routeModel.UkPrn);

                    return View("ApprenticeshipTraining", model);
                }

                var cachedReservation = await _mediator.Send(new GetCachedReservationQuery { Id = routeModel.Id.GetValueOrDefault() });

                if (isProvider)
                {
                    var courseCommand = new CacheReservationCourseCommand
                    {
                        Id = cachedReservation.Id,
                        SelectedCourseId = formModel.SelectedCourseId,
                        UkPrn = routeModel.UkPrn
                    };

                    await _mediator.Send(courseCommand);
                }

                var startDateCommand = new CacheReservationStartDateCommand
                {
                    Id = cachedReservation.Id,
                    TrainingDate = trainingDateModel,
                    UkPrn = routeModel.UkPrn
                };

                await _mediator.Send(startDateCommand);
            }
            catch (ValidationException e)
            {
                foreach (var member in e.ValidationResult.MemberNames)
                {
                    ModelState.AddModelError(member.Split('|')[0], member.Split('|')[1]);
                }

                var model = await BuildApprenticeshipTrainingViewModel(
                    isProvider,
                    formModel.AccountLegalEntityPublicHashedId,
                    employerAccountId,
                    formModel.SelectedCourseId,
                    trainingDateModel,
                    formModel.FromReview,
                    formModel.CohortRef,
                    routeModel.UkPrn);

                return View("ApprenticeshipTraining", model);
            }
            catch (CachedReservationNotFoundException ex)
            {
                _logger.LogWarning(ex, "Expected a cached reservation but did not find one.");
                return RedirectToRoute(routeModel.UkPrn.HasValue ? RouteNames.ProviderIndex : RouteNames.EmployerIndex, routeModel);
            }

            var reviewRouteName = isProvider ?
                RouteNames.ProviderReview :
                RouteNames.EmployerReview;

            return RedirectToRoute(reviewRouteName, routeModel);
        }

        [Route("{ukPrn}/reservations/{id}/review", Name = RouteNames.ProviderReview)]
        [Route("accounts/{employerAccountId}/reservations/{id}/review", Name = RouteNames.EmployerReview)]
        public async Task<IActionResult> Review(ReservationsRouteModel routeModel)
        {
            GetCachedReservationResult cachedReservation;

            try
            {
                var query = new GetCachedReservationQuery
                {
                    Id = routeModel.Id.GetValueOrDefault()
                };

                cachedReservation = await _mediator.Send(query);
            }
            catch (ValidationException e)
            {
                var errors = new StringBuilder();
                errors.AppendLine();
                foreach (var member in e.ValidationResult.MemberNames)
                {
                    errors.AppendLine(member);
                }

                _logger.LogWarning($"Validation Error when reviewing a reservation: {errors}");

                return RedirectToRoute(RouteNames.Error500);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return RedirectToRoute(RouteNames.Error500);
            }

            routeModel.FromReview = true;

            var viewModel = new ReviewViewModel(
                routeModel,
                cachedReservation.TrainingDate,
                cachedReservation.CourseDescription,
                cachedReservation.AccountLegalEntityName,
                cachedReservation.AccountLegalEntityPublicHashedId);

            return View(viewModel.ViewName, viewModel);
        }

        [Route("{ukPrn}/reservations/{id}/review", Name = RouteNames.ProviderPostReview)]
        [Route("accounts/{employerAccountId}/reservations/{id}/review", Name = RouteNames.EmployerPostReview)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PostReview(ReservationsRouteModel routeModel, PostReviewViewModel viewModel)
        {
            var isProvider = routeModel.UkPrn.HasValue;
            var reviewViewName = isProvider ? ViewNames.ProviderReview : ViewNames.EmployerReview;
            try
            {
                if (!isProvider)
                {
                    if (!ModelState.IsValid)
                    {
                        var reviewViewModel = new ReviewViewModel(routeModel, viewModel);
                        return View(reviewViewName, reviewViewModel);
                    }

                    if (!viewModel.Reserve.Value)
                    {
                        var homeUrl = _urlHelper.GenerateDashboardUrl(routeModel.EmployerAccountId);
                        return Redirect(homeUrl);
                    }
                }

                Guid? userId = null;
                if (!isProvider)
                {
                    var userAccountIdClaim = HttpContext.User.Claims.First(c => c.Type.Equals(EmployerClaims.IdamsUserIdClaimTypeIdentifier));

                    userId = Guid.Parse(userAccountIdClaim.Value);
                }

                var command = new CreateReservationCommand
                {
                    Id = routeModel.Id.GetValueOrDefault(),
                    UkPrn = routeModel.UkPrn,
                    UserId = userId
                };

                var result = await _mediator.Send(command);
                routeModel.AccountLegalEntityPublicHashedId = result.AccountLegalEntityPublicHashedId;
                routeModel.CohortReference = result.CohortRef;
                routeModel.JourneyData = result.JourneyData;


                if (result.IsEmptyCohortFromSelect)
                {
                    routeModel.ProviderId = result.ProviderId;
                }
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning(ex, "Validation error when trying to create reservation from cached reservation.");
                return RedirectToRoute(routeModel.UkPrn.HasValue ? RouteNames.ProviderIndex : RouteNames.EmployerIndex, routeModel);
            }
            catch (CachedReservationNotFoundException ex)
            {
                _logger.LogWarning(ex, "Expected a cached reservation but did not find one.");
                return RedirectToRoute(routeModel.UkPrn.HasValue ? RouteNames.ProviderIndex : RouteNames.EmployerIndex, routeModel);
            }

            return RedirectToRoute(routeModel.UkPrn.HasValue ? RouteNames.ProviderCompleted : RouteNames.EmployerCompleted, routeModel);
        }

        // GET
        [Route("{ukPrn}/reservations/{id}/completed/{accountLegalEntityPublicHashedId}", Name = RouteNames.ProviderCompleted)]
        [Route("accounts/{employerAccountId}/reservations/{id}/completed/{accountLegalEntityPublicHashedId}", Name = RouteNames.EmployerCompleted)]
        public async Task<IActionResult> Completed(ReservationsRouteModel routeModel)
        {
            if (!routeModel.Id.HasValue)
            {
                throw new ArgumentException("Reservation ID must be in URL.", nameof(routeModel.Id));
            }

            var query = new GetReservationQuery
            {
                Id = routeModel.Id.Value,
                UkPrn = routeModel.UkPrn.GetValueOrDefault()
            };
            var queryResult = await _mediator.Send(query);
            //todo: null check on result, redirect to error

            var model = new CompletedViewModel
            {
                AccountLegalEntityName = queryResult.AccountLegalEntityName,
                TrainingDateDescription = new TrainingDateModel()
                {
                    StartDate = queryResult.StartDate,
                    EndDate = queryResult.ExpiryDate
                }.GetGDSDateString(),
                CourseDescription = queryResult.Course.CourseDescription,
                StartDate = queryResult.StartDate,
                CourseId = queryResult.Course?.Id,
                UkPrn = queryResult.UkPrn ?? routeModel.ProviderId,
                CohortRef = routeModel.CohortReference,
                JourneyData = routeModel.JourneyData
            };

            var viewName = routeModel.UkPrn.HasValue ? ViewNames.ProviderCompleted : ViewNames.EmployerCompleted;
            return View(viewName, model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("{ukPrn}/reservations/{id}/completed/{accountLegalEntityPublicHashedId}", Name = RouteNames.ProviderPostCompleted)]
        [Route("accounts/{employerAccountId}/reservations/{id}/completed/{accountLegalEntityPublicHashedId}", Name = RouteNames.EmployerPostCompleted)]
        public IActionResult PostCompleted(ReservationsRouteModel routeModel, CompletedViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var viewName = routeModel.UkPrn.HasValue ? ViewNames.ProviderCompleted : ViewNames.EmployerCompleted;
                return View(viewName, model);
            }

            switch (model.WhatsNext)
            {
                case CompletedReservationWhatsNext.RecruitAnApprentice:
                    var recruitUrl = routeModel.UkPrn.HasValue
                        ? _urlHelper.GenerateUrl(new UrlParameters
                        {
                            SubDomain = "recruit",
                            Id = routeModel.UkPrn.ToString()
                        })
                        : _urlHelper.GenerateUrl(new UrlParameters
                        {
                            SubDomain = "recruit",
                            Folder = "accounts",
                            Id = routeModel.EmployerAccountId
                        });

                    return Redirect(recruitUrl);

                case CompletedReservationWhatsNext.FindApprenticeshipTraining:
                    return Redirect(_configuration.FindApprenticeshipTrainingUrl);

                case CompletedReservationWhatsNext.AddAnApprentice:
                    var addApprenticeUrl = _urlHelper.GenerateAddApprenticeUrl(routeModel.Id.Value,
                        routeModel.AccountLegalEntityPublicHashedId, model.CourseId, model.UkPrn, model.StartDate,
                        model.CohortRef, routeModel.EmployerAccountId, routeModel.UkPrn == null && model.UkPrn != null,
                        journeyData: model.JourneyData);
                    return Redirect(addApprenticeUrl);

                default:
                    var homeUrl = _urlHelper.GenerateDashboardUrl(routeModel.EmployerAccountId);
                    return Redirect(homeUrl);
            }
        }


        private async Task<ApprenticeshipTrainingViewModel> BuildApprenticeshipTrainingViewModel(
            bool isProvider,
            string accountLegalEntityPublicHashedId,
            long accountId,
            string courseId = null,
            TrainingDateModel selectedTrainingDate = null,
            bool? routeModelFromReview = false,
            string cohortRef = "",
            uint? ukPrn = null)

        {
            var hashedEmployerAccountId = _encodingService.Encode(accountId, EncodingType.AccountId);
            var accountLegalEntityId = _encodingService.Decode(
                accountLegalEntityPublicHashedId,
                EncodingType.PublicAccountLegalEntityId);

            var coursesResult = await _mediator.Send(new GetCoursesQuery());

            var activeGlobalRule = await GetActiveGlobalRule(accountId);

            var dates = await _trainingDateService.GetTrainingDates(accountLegalEntityId);

            var possibleDates = activeGlobalRule == null
                ? dates.Select(startDateModel => new TrainingDateViewModel(startDateModel, startDateModel.Equals(selectedTrainingDate))).OrderBy(model => model.StartDate)
                : dates.Where(d => d.StartDate >= activeGlobalRule.ActiveTo).Select(startDateModel => new TrainingDateViewModel(startDateModel, startDateModel.Equals(selectedTrainingDate))).OrderBy(model => model.StartDate);

            return new ApprenticeshipTrainingViewModel
            {
                ActiveGlobalRule = new GlobalRuleViewModel(activeGlobalRule),
                RouteName = isProvider ? RouteNames.ProviderCreateApprenticeshipTraining : RouteNames.EmployerCreateApprenticeshipTraining,
                PossibleStartDates = possibleDates,
                Courses = coursesResult.Courses?.Select(course => new CourseViewModel(course, courseId)),
                CourseId = courseId,
                AccountLegalEntityPublicHashedId = accountLegalEntityPublicHashedId,
                IsProvider = isProvider,
                CohortRef = cohortRef,
                FromReview = routeModelFromReview,
                BackLink = isProvider ?
                    GetProviderBackLinkForApprenticeshipTrainingView(routeModelFromReview, cohortRef, ukPrn, hashedEmployerAccountId)
                    : routeModelFromReview.HasValue && routeModelFromReview.Value ? RouteNames.EmployerReview : RouteNames.EmployerSelectCourse
            };
        }

        private string GetProviderBackLinkForApprenticeshipTrainingView(bool? routeModelFromReview, string cohortRef, uint? ukPrn, string accountId)
        {
            if (string.IsNullOrEmpty(cohortRef))
            {
                return routeModelFromReview.HasValue && routeModelFromReview.Value ? RouteNames.ProviderReview : RouteNames.ProviderConfirmEmployer;
            }

            return _urlHelper.GenerateCohortDetailsUrl(ukPrn, accountId, cohortRef);

        }

        private async Task<GlobalRule> GetActiveGlobalRule(long? employerAccountId = null)
        {
            if (employerAccountId.HasValue)
            {
                return (await _mediator.Send(new GetAccountFundingRulesQuery { AccountId = employerAccountId.Value }))?.ActiveRule;
            }

            return null;
        }
    }
}
