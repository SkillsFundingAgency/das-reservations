using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<EmployerReservationsController> _logger;
        private readonly ReservationsWebConfiguration _config;

        public EmployerReservationsController(
            IMediator mediator, 
            IEncodingService encodingService, 
            IOptions<ReservationsWebConfiguration> options, 
            IExternalUrlHelper urlHelper,
            ILogger<EmployerReservationsController> logger)
        {
            _mediator = mediator;
            _encodingService = encodingService;
            _urlHelper = urlHelper;
            _logger = logger;
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
            catch (ValidationException e)
            {
                _logger.LogInformation(e, e.Message);
                return RedirectToRoute(RouteNames.Error500);
            }
        }
            
        [HttpGet]
        [Route("select-legal-entity/{id?}", Name = RouteNames.EmployerSelectLegalEntity)]
        public async Task<IActionResult> SelectLegalEntity(ReservationsRouteModel routeModel)
        {
            try
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
                    await CacheReservation(routeModel, accountLegalEntity, true);
                    return RedirectToRoute(RouteNames.EmployerSelectCourse, routeModel);
                }

                var viewModel = new SelectLegalEntityViewModel(routeModel, legalEntitiesResponse.AccountLegalEntities, cachedResponse?.AccountLegalEntityPublicHashedId);
                return View("SelectLegalEntity", viewModel);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return RedirectToRoute(RouteNames.Error500);
            }
        }

        private async Task CacheReservation(ReservationsRouteModel routeModel, AccountLegalEntity accountLegalEntity, bool employerHasSingleLegalEntity = false)
        {
            var reservationId = routeModel.Id ?? Guid.NewGuid();
            
            await _mediator.Send(new CacheReservationEmployerCommand
            {
                Id = reservationId,
                AccountId = accountLegalEntity.AccountId,
                AccountLegalEntityId = accountLegalEntity.AccountLegalEntityId,
                AccountLegalEntityName = accountLegalEntity.AccountLegalEntityName,
                AccountLegalEntityPublicHashedId = accountLegalEntity.AccountLegalEntityPublicHashedId,
                EmployerHasSingleLegalEntity = employerHasSingleLegalEntity
            });

            routeModel.Id = reservationId;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("select-legal-entity/{id?}", Name = RouteNames.EmployerSelectLegalEntity)]
        public async Task<IActionResult> PostSelectLegalEntity(ReservationsRouteModel routeModel, ConfirmLegalEntityViewModel viewModel)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var legalEntitiesResponse = await _mediator.Send(new GetLegalEntitiesQuery { AccountId = _encodingService.Decode(routeModel.EmployerAccountId, EncodingType.AccountId) });
                    var requestViewModel = new SelectLegalEntityViewModel(routeModel, legalEntitiesResponse.AccountLegalEntities, viewModel.LegalEntity);
                    return View("SelectLegalEntity", requestViewModel);
                }

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
            var viewModel = await BuildEmployerSelectCourseViewModel(routeModel, routeModel.FromReview);

            if (viewModel == null)
            {
                return View("Index");
            }

            return View("SelectCourse",viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("{id}/select-course", Name = RouteNames.EmployerSelectCourse)]
        public async Task<IActionResult> PostSelectCourse(ReservationsRouteModel routeModel, PostSelectCourseViewModel postViewModel)
        {
            if (!ModelState.IsValid)
            {

                var viewModel = await BuildEmployerSelectCourseViewModel(routeModel, postViewModel.ApprenticeTrainingKnown);

                if (viewModel == null)
                {
                    return View("Index");
                }

                return View("SelectCourse",viewModel);
            }

            if (postViewModel.ApprenticeTrainingKnown == false)
            {
                return RedirectToRoute(RouteNames.EmployerCourseGuidance, routeModel);
            }

            try
            {
                await _mediator.Send(new CacheReservationCourseCommand
                {
                    Id = routeModel.Id.Value,
                    CourseId = postViewModel.SelectedCourseId

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

                var viewModel = await BuildEmployerSelectCourseViewModel(routeModel, postViewModel.ApprenticeTrainingKnown, true);


                return View("SelectCourse", viewModel);
            }
            catch (CachedReservationNotFoundException)
            {
                throw new ArgumentException("Reservation not found", nameof(routeModel.Id));
            }
        }

        [HttpGet]
        [Route("{id}/course-guidance", Name = RouteNames.EmployerCourseGuidance)]
        public IActionResult CourseGuidance(ReservationsRouteModel routeModel)
        {
            var model = new CourseGuidanceViewModel
            {
                DashboardUrl = _urlHelper.GenerateDashboardUrl(routeModel.EmployerAccountId),
                BackRouteName = RouteNames.EmployerSelectCourse,
                ProviderPermissionsUrl = _urlHelper.GenerateUrl(new UrlParameters
                {
                    SubDomain = "permissions",
                    Controller = "providers",
                    Id = routeModel.EmployerAccountId,
                    Folder = "accounts"
                }),
                FindApprenticeshipTrainingUrl = _config.FindApprenticeshipTrainingUrl
            };


            return View("CourseGuidance", model);
        }

        private async Task<EmployerSelectCourseViewModel> BuildEmployerSelectCourseViewModel(
            ReservationsRouteModel routeModel,
            bool? apprenticeTrainingKnownOrFromReview,
            bool failedValidation = false)
        {
            var cachedReservation = await _mediator.Send(new GetCachedReservationQuery { Id = routeModel.Id.Value });
            if (cachedReservation == null)
            {
                return null;
            }

            var getCoursesResponse = await _mediator.Send(new GetCoursesQuery());

            var courseViewModels = getCoursesResponse.Courses.Select(course => new CourseViewModel(course, failedValidation? null : cachedReservation.CourseId));

            var viewModel = new EmployerSelectCourseViewModel
            {
                ReservationId = routeModel.Id.Value,
                Courses = courseViewModels,
                BackLink = GenerateBackLink(routeModel, cachedReservation.CohortRef, cachedReservation.EmployerHasSingleLegalEntity),
                CohortReference = cachedReservation.CohortRef,
                ApprenticeTrainingKnown = !string.IsNullOrEmpty(cachedReservation.CourseId) ? true : apprenticeTrainingKnownOrFromReview
            };

            return viewModel;
        }

        private string GenerateBackLink(ReservationsRouteModel routeModel, string cohortRef, bool employerHasSingleLegalEntity = false)
        {
            if (!string.IsNullOrEmpty(routeModel.CohortReference))
            {
                return _urlHelper.GenerateCohortDetailsUrl(null, routeModel.EmployerAccountId, cohortRef);
            }

            if (routeModel.FromReview.HasValue && routeModel.FromReview.Value)
                return RouteNames.EmployerReview;
            
            if (employerHasSingleLegalEntity)
                return RouteNames.EmployerStart;

            return RouteNames.EmployerSelectLegalEntity;
        }

        private string GenerateLimitReachedBackLink(ReservationsRouteModel routeModel)
        {
            if (!string.IsNullOrEmpty(routeModel.CohortReference))
            {
                return _urlHelper.GenerateCohortDetailsUrl(null, routeModel.EmployerAccountId, routeModel.CohortReference);
            }

            return Url.RouteUrl(RouteNames.EmployerManage, routeModel);
        }
    }
}
