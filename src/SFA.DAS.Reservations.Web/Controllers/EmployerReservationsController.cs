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
using SFA.DAS.Reservations.Application.Employers.Queries.GetAccountUsers;
using SFA.DAS.Reservations.Application.Employers.Queries.GetLegalEntities;
using SFA.DAS.Reservations.Application.Exceptions;
using SFA.DAS.Reservations.Application.FundingRules.Queries.GetAccountFundingRules;
using SFA.DAS.Reservations.Application.Reservations.Commands.CacheReservationCourse;
using SFA.DAS.Reservations.Application.Reservations.Commands.CacheReservationEmployer;
using SFA.DAS.Reservations.Application.Reservations.Queries.GetCachedReservation;
using SFA.DAS.Reservations.Application.Reservations.Queries.GetCourses;
using SFA.DAS.Reservations.Domain.Employers;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Domain.Rules;
using SFA.DAS.Reservations.Infrastructure.Configuration;
using SFA.DAS.Reservations.Infrastructure.Exceptions;
using SFA.DAS.Reservations.Web.Filters;
using SFA.DAS.Reservations.Web.Infrastructure;
using SFA.DAS.Reservations.Web.Models;
using SFA.DAS.Reservations.Web.Services;

namespace SFA.DAS.Reservations.Web.Controllers
{
    [Authorize(Policy = nameof(PolicyNames.HasEmployerAccount))]
    [ServiceFilter(typeof(LevyNotPermittedFilter))]
    [Route("accounts/{employerAccountId}/reservations", Name = RouteNames.EmployerIndex)]
    public class EmployerReservationsController : ReservationsBaseController
    {
        private readonly IMediator _mediator;
        private readonly IEncodingService _encodingService;
        private readonly IExternalUrlHelper _urlHelper;
        private readonly ILogger<EmployerReservationsController> _logger;
        private readonly IUserClaimsService _userClaimsService;
        private readonly ReservationsWebConfiguration _config;

        public EmployerReservationsController(
            IMediator mediator, 
            IEncodingService encodingService, 
            IOptions<ReservationsWebConfiguration> options, 
            IExternalUrlHelper urlHelper,
            ILogger<EmployerReservationsController> logger,
            IUserClaimsService userClaimsService) : base(mediator)
        {
            _mediator = mediator;
            _encodingService = encodingService;
            _urlHelper = urlHelper;
            _logger = logger;
            _userClaimsService = userClaimsService;
            _config = options.Value;
        }

        // GET
        [ServiceFilter(typeof(NonEoiNotPermittedFilterAttribute))]
        public async Task<IActionResult> Index(ReservationsRouteModel routeModel)
        {

            var viewResult = await CheckNextGlobalRule(RouteNames.EmployerStart, EmployerClaims.IdamsUserIdClaimTypeIdentifier, Url.RouteUrl(RouteNames.EmployerManage), RouteNames.EmployerSaveRuleNotificationChoiceNoReservation);

            if (viewResult == null)
            {
                return RedirectToRoute(RouteNames.EmployerStart, routeModel);
            }

            return viewResult;

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
                        return View("EmployerFundingPaused", GenerateLimitReachedBackLink(routeModel));

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

                    if (!accountLegalEntity.AgreementSigned)
                    {
                        return RedirectToSignAgreement(routeModel, RouteNames.EmployerIndex);
                    }

                    await CacheReservation(routeModel, accountLegalEntity, true);
                    return RedirectToRoute(RouteNames.EmployerSelectCourse, routeModel);
                }

                var viewModel = new SelectLegalEntityViewModel(routeModel, legalEntitiesResponse.AccountLegalEntities, cachedResponse?.AccountLegalEntityPublicHashedId);
                return View("SelectLegalEntity", viewModel);
            }
            catch (ReservationLimitReachedException)
            {
                return View("ReservationLimitReached", GenerateLimitReachedBackLink(routeModel));
            }
            catch (GlobalReservationRuleException)
            {
                return View("EmployerFundingPaused", GenerateLimitReachedBackLink(routeModel));
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return RedirectToRoute(RouteNames.Error500);
            }
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

                if (!selectedAccountLegalEntity.AgreementSigned)
                {
                    return RedirectToSignAgreement(routeModel, RouteNames.EmployerSelectLegalEntity);
                }

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
            catch (GlobalReservationRuleException)
            {
                return View("EmployerFundingPaused");
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
                    SelectedCourseId = postViewModel.SelectedCourseId

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

        [HttpGet]
        [Route("owner-sign-agreement", Name = RouteNames.EmployerOwnerSignAgreement)]
        public IActionResult OwnerSignAgreement(ReservationsRouteModel routeModel)
        {
            var model = new SignAgreementViewModel
            {
                BackRouteName = routeModel.PreviousPage
            };
            
            return View("OwnerSignAgreement", model);
        }

        [HttpGet]
        [Route("transactor-sign-agreement", Name = RouteNames.EmployerTransactorSignAgreement)]
        public async Task<IActionResult> TransactorSignAgreement(ReservationsRouteModel routeModel)
        {
            try
            {
                var decodedAccountId = _encodingService.Decode(
                    routeModel.EmployerAccountId, 
                    EncodingType.AccountId);

                var users = await _mediator.Send(new GetAccountUsersQuery
                {
                    AccountId = decodedAccountId
                });

                var owners = users.AccountUsers
                    .Where(user => user.Role.Equals(EmployerUserRole.Owner.ToString(), StringComparison.InvariantCultureIgnoreCase))
                    .OrderBy(user => user.Name)
                    .Select(user => (EmployerAccountUserViewModel) user);

                var model = new SignAgreementViewModel
                {
                    BackRouteName = routeModel.PreviousPage,
                    OwnersOfThisAccount = owners
                };

                return View("TransactorSignAgreement", model);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error attempting to show the transactor sign agreement page.");
                return RedirectToRoute(RouteNames.Error500);
            }
        }

        private RedirectToRouteResult RedirectToSignAgreement(ReservationsRouteModel routeModel, string previousRouteName)
        {
            if (_userClaimsService.UserIsInRole(routeModel.EmployerAccountId,
                EmployerUserRole.Owner, User.Claims))
            {
                routeModel.PreviousPage = previousRouteName;
                return RedirectToRoute(RouteNames.EmployerOwnerSignAgreement, routeModel);
            }

            routeModel.PreviousPage = previousRouteName;
            return RedirectToRoute(RouteNames.EmployerTransactorSignAgreement, routeModel);
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
                BackLink = GenerateBackLink(routeModel, cachedReservation),
                CohortReference = cachedReservation.CohortRef,
                ApprenticeTrainingKnown = !string.IsNullOrEmpty(cachedReservation.CourseId) ? true : apprenticeTrainingKnownOrFromReview,
                IsEmptyCohortFromSelect = cachedReservation.IsEmptyCohortFromSelect
            };

            return viewModel;
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


        private string GenerateBackLink(ReservationsRouteModel routeModel, GetCachedReservationResult result)
        {
            if (!string.IsNullOrEmpty(result.CohortRef) || result.IsEmptyCohortFromSelect)
            {
                return _urlHelper.GenerateCohortDetailsUrl(result.UkPrn, routeModel.EmployerAccountId, result.CohortRef, result.IsEmptyCohortFromSelect);
            }

            if (routeModel.FromReview.HasValue && routeModel.FromReview.Value)
                return RouteNames.EmployerReview;
            
            if (result.EmployerHasSingleLegalEntity)
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
