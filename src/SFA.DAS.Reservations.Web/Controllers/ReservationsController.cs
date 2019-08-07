using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SFA.DAS.Authorization.CommitmentPermissions.Options;
using SFA.DAS.Authorization.Mvc.Attributes;
using SFA.DAS.Encoding;
using SFA.DAS.Reservations.Application.Employers.Queries;
using SFA.DAS.Reservations.Application.Employers.Queries.GetLegalEntities;
using SFA.DAS.Reservations.Application.Exceptions;
using SFA.DAS.Reservations.Application.FundingRules.Queries.GetNextUnreadGlobalFundingRule;
using SFA.DAS.Reservations.Application.Providers.Queries.GetLegalEntityAccount;
using SFA.DAS.Reservations.Application.Reservations.Commands.CacheReservationCourse;
using SFA.DAS.Reservations.Application.Reservations.Commands.CacheReservationEmployer;
using SFA.DAS.Reservations.Application.Reservations.Commands.CacheReservationStartDate;
using SFA.DAS.Reservations.Application.Reservations.Commands.CreateReservation;
using SFA.DAS.Reservations.Application.Reservations.Commands.CreateReservationLevyEmployer;
using SFA.DAS.Reservations.Application.Reservations.Commands.DeleteReservation;
using SFA.DAS.Reservations.Application.Reservations.Queries.GetAccountReservationStatus;
using SFA.DAS.Reservations.Application.Reservations.Queries.GetAvailableReservations;
using SFA.DAS.Reservations.Application.Reservations.Queries.GetCachedReservation;
using SFA.DAS.Reservations.Application.Reservations.Queries.GetCourses;
using SFA.DAS.Reservations.Application.Reservations.Queries.GetProviderCacheReservationCommand;
using SFA.DAS.Reservations.Application.Reservations.Queries.GetReservation;
using SFA.DAS.Reservations.Application.Reservations.Queries.GetReservations;
using SFA.DAS.Reservations.Domain.Courses;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Domain.Rules;
using SFA.DAS.Reservations.Domain.Rules.Api;
using SFA.DAS.Reservations.Infrastructure.Configuration;
using SFA.DAS.Reservations.Infrastructure.Exceptions;
using SFA.DAS.Reservations.Web.Extensions;
using SFA.DAS.Reservations.Web.Infrastructure;
using SFA.DAS.Reservations.Web.Models;
using SFA.DAS.Reservations.Web.Services;

namespace SFA.DAS.Reservations.Web.Controllers
{
    [Authorize(Policy = nameof(PolicyNames.HasProviderOrEmployerAccount))]
    public class ReservationsController : Controller
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
            IExternalUrlHelper urlHelper)
        {
            _mediator = mediator;
            _trainingDateService = trainingDateService;
            _logger = logger;
            _encodingService = encodingService;
            _configuration = configuration.Value;
            _urlHelper = urlHelper;
        }

        [Route("{ukPrn}/reservations/{id}/apprenticeship-training/{fromReview?}", Name = RouteNames.ProviderApprenticeshipTraining)]
        [Route("accounts/{employerAccountId}/reservations/{id}/apprenticeship-training", Name = RouteNames.EmployerApprenticeshipTraining)]
        public async Task<IActionResult> ApprenticeshipTraining(ReservationsRouteModel routeModel)
        {
            GetCachedReservationResult cachedReservation = null;

            if (routeModel.Id.HasValue)
            {
                cachedReservation = await _mediator.Send(new GetCachedReservationQuery {Id = routeModel.Id.GetValueOrDefault()});
                //todo: error handling if fails validation e.g. id not found, redirect to index.
            }
            
            var viewModel = await BuildApprenticeshipTrainingViewModel(
                routeModel.UkPrn != null, 
                cachedReservation?.AccountLegalEntityPublicHashedId, 
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
            Course course = null;

            try
            {
                if (!string.IsNullOrWhiteSpace(formModel.StartDate))
                    trainingDateModel = JsonConvert.DeserializeObject<TrainingDateModel>(formModel.StartDate);

                if (!ModelState.IsValid)
                {
                    var model = await BuildApprenticeshipTrainingViewModel(
                        isProvider, 
                        formModel.AccountLegalEntityPublicHashedId, 
                        formModel.SelectedCourseId, 
                        trainingDateModel,
                        formModel.FromReview,
                        formModel.CohortRef,
                        routeModel.UkPrn);
                       
                    return View("ApprenticeshipTraining", model);
                }

                if (!string.IsNullOrEmpty(formModel.SelectedCourseId))
                {
                    var getCoursesResult = await _mediator.Send(new GetCoursesQuery());

                    var selectedCourse =
                        getCoursesResult.Courses.SingleOrDefault(c => c.Id.Equals(formModel.SelectedCourseId));

                    course = selectedCourse ?? throw new ArgumentException("Selected course does not exist", nameof(formModel.SelectedCourseId));
                    //todo: should be a validation exception, also this throw is not unit tested
                }

		 		var cachedReservation = await _mediator.Send(new GetCachedReservationQuery {Id = routeModel.Id.GetValueOrDefault()});

                if(isProvider)
				{             
	                var courseCommand = new CacheReservationCourseCommand
	                {
	                    Id = cachedReservation.Id,
	                    CourseId = course?.Id,
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
                    formModel.SelectedCourseId,
                    trainingDateModel);
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
                foreach (var member in e.ValidationResult.MemberNames)
                {
                    ModelState.AddModelError(member.Split('|')[0], member.Split('|')[1]);
                }

                return View("Error");//todo: setup view correctly.
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
        public async Task<IActionResult> PostReview(ReservationsRouteModel routeModel)
        {
            try
            {
                var command = new CreateReservationCommand
                {
                    Id = routeModel.Id.GetValueOrDefault(),
                    UkPrn = routeModel.UkPrn
                };

                var result = await _mediator.Send(command);
                routeModel.AccountLegalEntityPublicHashedId = result.AccountLegalEntityPublicHashedId;
                routeModel.CohortReference = result.CohortRef;
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
                UkPrn = queryResult.UkPrn,
                CohortRef = routeModel.CohortReference
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
                        ? _urlHelper.GenerateUrl(new UrlParameters {
                            SubDomain = "recruit", 
                            Id = routeModel.UkPrn.ToString()
                        })
                        : _urlHelper.GenerateUrl(new UrlParameters {
                            SubDomain = "recruit", 
                            Folder = "accounts",
                            Id = routeModel.EmployerAccountId
                        });
                        
                    return Redirect(recruitUrl);

                case CompletedReservationWhatsNext.FindApprenticeshipTraining:
                    return Redirect(_configuration.FindApprenticeshipTrainingUrl);

                case CompletedReservationWhatsNext.AddAnApprentice:
                    var addApprenticeUrl = _urlHelper.GenerateAddApprenticeUrl(
                        routeModel.Id.Value, 
                        routeModel.AccountLegalEntityPublicHashedId, model.CourseId, 
                        routeModel.UkPrn,
                        model.StartDate, 
                        model.CohortRef,
                        routeModel.EmployerAccountId);

                    return Redirect(addApprenticeUrl);

                default:
                    var homeUrl = routeModel.UkPrn.HasValue
                        ? _urlHelper.GenerateUrl(new UrlParameters {Controller = "account"})
                        : _urlHelper.GenerateUrl(new UrlParameters {
                            Folder = "accounts",
                            Controller = "teams",
                            Id = routeModel.EmployerAccountId
                        });
                    return Redirect(homeUrl);
            }
        }

        [Route("{ukPrn}/reservations/manage", Name = RouteNames.ProviderManage)]
        [Route("accounts/{employerAccountId}/reservations/manage", Name = RouteNames.EmployerManage)]
        public async Task<IActionResult> Manage(ReservationsRouteModel routeModel)
        {
            var employerAccountIds = new List<long>();
            var reservations = new List<ReservationViewModel>();
            string viewName;

            if (routeModel.UkPrn.HasValue)
            {
                var trustedEmployersResponse = await _mediator.Send(new GetTrustedEmployersQuery { UkPrn = routeModel.UkPrn.Value });

                if (!trustedEmployersResponse.Employers.Any())
                {
                    return View("NoPermissions");
                }

                employerAccountIds.AddRange(trustedEmployersResponse.Employers.Select(employer => employer.AccountId));
                viewName = ViewNames.ProviderManage;
            }
            else
            {
                var decodedAccountId = _encodingService.Decode(routeModel.EmployerAccountId, EncodingType.AccountId);
                employerAccountIds.Add(decodedAccountId);
                viewName = ViewNames.EmployerManage;
            }

            foreach (var employerAccountId in employerAccountIds)
            {
                var reservationsResult = await _mediator.Send(new GetReservationsQuery{AccountId = employerAccountId});

                foreach (var reservation in reservationsResult.Reservations)
                {
                    if (routeModel.UkPrn.HasValue)
                    {
                        reservation.ProviderId = routeModel.UkPrn;
                    }

                    var accountLegalEntityPublicHashedId = _encodingService.Encode(reservation.AccountLegalEntityId,
                        EncodingType.PublicAccountLegalEntityId);

                    var apprenticeUrl = _urlHelper.GenerateAddApprenticeUrl(
                        reservation.Id, 
                        accountLegalEntityPublicHashedId, 
                        reservation.Course.Id,
                        routeModel.UkPrn,
                        reservation.StartDate,
                        routeModel.CohortReference,
                        routeModel.EmployerAccountId);

                    var viewModel = new ReservationViewModel(reservation, apprenticeUrl);

                    reservations.Add(viewModel);
                }
            }
            
            return View(viewName, new ManageViewModel
            {
                Reservations = reservations,
                BackLink = routeModel.UkPrn.HasValue
                    ? _urlHelper.GenerateUrl(new UrlParameters{ Controller = "Account"})
                    : _urlHelper.GenerateUrl( new UrlParameters {
                        Controller = "teams", 
                        SubDomain = "accounts", 
                        Folder = "accounts", 
                        Id = routeModel.EmployerAccountId
                    })
            });
        }

        [Route("{ukPrn}/reservations/manage/create", Name = RouteNames.ProviderManageCreate)]
        [Route("accounts/{employerAccountId}/reservations/manage/create", Name = RouteNames.EmployerManageCreate)]
        public async Task<IActionResult> CreateReservation(ReservationsRouteModel routeModel)
        {
            string userId;

            if (routeModel.UkPrn.HasValue)
            {
                var providerUkPrnClaim = HttpContext.User.Claims.First(c => c.Type.Equals(ProviderClaims.ProviderUkprn));
                userId = providerUkPrnClaim.Value;
            }
            else
            {
                var userAccountIdClaim = HttpContext.User.Claims.First(c => c.Type.Equals(EmployerClaims.IdamsUserIdClaimTypeIdentifier));
                userId = userAccountIdClaim.Value;
            }
           
            var response = await _mediator.Send(new GetNextUnreadGlobalFundingRuleQuery{Id = userId});

            var nextGlobalRuleId = response?.Rule?.Id;
            var nextGlobalRuleStartDate = response?.Rule?.ActiveFrom;

            if (!nextGlobalRuleId.HasValue || nextGlobalRuleId.Value == 0|| !nextGlobalRuleStartDate.HasValue)
            {
                if (routeModel.UkPrn.HasValue)
                {
                    return RedirectToRoute(RouteNames.ProviderStart, routeModel);
                }

                return RedirectToRoute(RouteNames.EmployerStart);
            }

            var viewModel = new FundingRestrictionNotificationViewModel
            {
                RuleId = nextGlobalRuleId.Value,
                TypeOfRule = RuleType.GlobalRule,
                RestrictionStartDate = nextGlobalRuleStartDate.Value
            };

            if (routeModel.UkPrn.HasValue)
            {
                viewModel.BackLink = RouteNames.ProviderManage;

                return View("../ProviderReservations/FundingRestrictionNotification", viewModel);
            }

            viewModel.BackLink = RouteNames.EmployerManage;

            return View("../EmployerReservations/FundingRestrictionNotification", viewModel);
        }

        [Route("{ukPrn}/reservations/{id}/delete", Name = RouteNames.ProviderDelete)]
        [Route("accounts/{employerAccountId}/reservations/{id}/delete", Name = RouteNames.EmployerDelete)]
        public async Task<IActionResult> Delete(ReservationsRouteModel routeModel)
        {
            var isProvider = routeModel.UkPrn.HasValue;
            try
            {
                if (!routeModel.Id.HasValue)
                {
                    _logger.LogInformation($"Reservation ID must be in URL, parameter [{nameof(routeModel.Id)}]");
                    var manageRoute = isProvider ? RouteNames.ProviderManage : RouteNames.EmployerManage;
                    return RedirectToRoute(manageRoute, routeModel);
                }

                var query = new GetReservationQuery
                {
                    Id = routeModel.Id.Value,
                    UkPrn = routeModel.UkPrn.GetValueOrDefault()
                };
                var queryResult = await _mediator.Send(query);

                var viewName = isProvider ? ViewNames.ProviderDelete : ViewNames.EmployerDelete;

                return View(viewName, new DeleteViewModel(queryResult));
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error preparing for the delete view");
                throw;
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("{ukPrn}/reservations/{id}/delete", Name = RouteNames.ProviderDelete)]
        [Route("accounts/{employerAccountId}/reservations/{id}/delete", Name = RouteNames.EmployerDelete)]
        public async Task<IActionResult> PostDelete(ReservationsRouteModel routeModel, DeleteViewModel viewModel)
        {
            var isProvider = routeModel.UkPrn.HasValue;
            var deleteViewName = isProvider ? ViewNames.ProviderDelete : ViewNames.EmployerDelete;
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(deleteViewName, viewModel);
                }

                if (viewModel.Delete.HasValue && !viewModel.Delete.Value ||
                    !routeModel.Id.HasValue)
                {
                    var manageRoute = isProvider ? RouteNames.ProviderManage : RouteNames.EmployerManage;
                    return RedirectToRoute(manageRoute, routeModel);
                }
                
                await _mediator.Send(new DeleteReservationCommand{ReservationId = routeModel.Id.Value});

                var completedRoute = isProvider ? RouteNames.ProviderDeleteCompleted : RouteNames.EmployerDeleteCompleted;
                return RedirectToRoute(completedRoute, routeModel);
            }
            catch (ValidationException ex)
            {
                _logger.LogInformation(ex, $"Validation error trying to delete reservation [{routeModel.Id}]");
                return View(deleteViewName, viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error trying to delete reservation [{routeModel.Id}]");
                var errorRoute = isProvider ? RouteNames.ProviderError : RouteNames.EmployerError;
                return RedirectToRoute(errorRoute, routeModel);
            }
        }

        [Route("{ukPrn}/reservations/{id}/delete-completed", Name = RouteNames.ProviderDeleteCompleted)]
        [Route("accounts/{employerAccountId}/reservations/{id}/delete-completed", Name = RouteNames.EmployerDeleteCompleted)]
        public IActionResult DeleteCompleted(ReservationsRouteModel routeModel)
        {
            var viewName = routeModel.UkPrn.HasValue ? ViewNames.ProviderDeleteCompleted : ViewNames.EmployerDeleteCompleted;

            return View(viewName);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("{ukPrn}/reservations/{id}/delete-completed", Name = RouteNames.ProviderDeleteCompleted)]
        [Route("accounts/{employerAccountId}/reservations/{id}/delete-completed", Name = RouteNames.EmployerDeleteCompleted)]
        public IActionResult PostDeleteCompleted(ReservationsRouteModel routeModel, DeleteCompletedViewModel viewModel)
        {
            var isProvider = routeModel.UkPrn.HasValue;
            var deleteCompletedViewName = isProvider ? ViewNames.ProviderDeleteCompleted : ViewNames.EmployerDeleteCompleted;
            var manageRouteName = isProvider ? RouteNames.ProviderManage : RouteNames.EmployerManage;
            var dashboardUrl = isProvider ? 
                _configuration.DashboardUrl : 
                _urlHelper.GenerateUrl(
                    new UrlParameters
                    {
                        Id = routeModel.EmployerAccountId,
                        Controller = "teams",
                        Folder = "accounts",
                        SubDomain = "accounts"
                    });
            
            if (!ModelState.IsValid)
            {
                return View(deleteCompletedViewName, viewModel);
            }

            if (viewModel.Manage.HasValue && viewModel.Manage.Value)
            {
                return RedirectToRoute(manageRouteName);
            }

            return Redirect(dashboardUrl);
        }

        [DasAuthorize(CommitmentOperation.AccessCohort)]
        [Route("{ukPrn}/reservations/{accountLegalEntityPublicHashedId}/select", Name = RouteNames.ProviderSelect)]
        [Route("accounts/{employerAccountId}/reservations/{accountLegalEntityPublicHashedId}/select", Name = RouteNames.EmployerSelect)]
        public async Task<IActionResult> SelectReservation(
            ReservationsRouteModel routeModel,
            SelectReservationViewModel viewModel)
        {
            var backUrl = string.Empty;

            try
            {   
                var viewName = ViewNames.EmployerSelect;
                var apprenticeshipTrainingRouteName = RouteNames.EmployerApprenticeshipTraining;
                CacheReservationEmployerCommand cacheReservationEmployerCommand;

                if (routeModel.UkPrn.HasValue)
                {
                    backUrl = _urlHelper.GenerateUrl(new UrlParameters
                    {
                        Id = routeModel.UkPrn.Value.ToString(),
                        Controller = $"apprentices/{viewModel.CohortReference}",
                        Action = "details"
                    });

                    try
                    {
                        var response = await _mediator.Send(new GetProviderCacheReservationCommandQuery
                        {
                            AccountLegalEntityPublicHashedId = routeModel.AccountLegalEntityPublicHashedId,
                            CohortRef = routeModel.CohortReference,
                            CohortId = _encodingService.Decode(routeModel.CohortReference, EncodingType.CohortReference),
                            UkPrn = routeModel.UkPrn.Value
                        });

                        cacheReservationEmployerCommand = response.Command;

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
                    
                    
                    viewName = ViewNames.ProviderSelect;
                    apprenticeshipTrainingRouteName = RouteNames.ProviderApprenticeshipTraining;
                    try
                    {
                        var autoReserveStatus = await _mediator.Send(
                            new GetAccountReservationStatusQuery
                            {
                                AccountId = cacheReservationEmployerCommand.AccountId,
                                TransferSenderAccountId = viewModel.TransferSenderId ?? "",
                                HashedEmployerAccountId = _encodingService.Encode(cacheReservationEmployerCommand.AccountId, EncodingType.AccountId)
                            });

                        if (autoReserveStatus != null && autoReserveStatus.CanAutoCreateReservations)
                        {
                            var createdReservation = await _mediator.Send(new CreateReservationLevyEmployerCommand
                            {
                                AccountId = cacheReservationEmployerCommand.AccountId,
                                TransferSenderId = autoReserveStatus.TransferAccountId == 0 ? (long?)null : autoReserveStatus.TransferAccountId,
                                AccountLegalEntityId = _encodingService.Decode(
                                    cacheReservationEmployerCommand.AccountLegalEntityPublicHashedId,
                                    EncodingType.PublicAccountLegalEntityId)
                            });

                            var addApprenticeUrl = _urlHelper.GenerateAddApprenticeUrl(createdReservation.ReservationId,
                                routeModel.AccountLegalEntityPublicHashedId, 
                                "", 
                                routeModel.UkPrn.Value, 
                                null, 
                                viewModel.CohortReference,
                                routeModel.EmployerAccountId);
                            
                            return Redirect(addApprenticeUrl);
                        }
                    }
                    catch (TransferSendNotAllowedException e)
                    {
                        _logger.LogWarning(e, $"AccountId: {e.AccountId} does not have sender id {e.TransferSenderId} allowed).");
                        return RedirectToRoute(RouteNames.Error500);
                    }
                }
                else
                {
                    cacheReservationEmployerCommand = await BuildEmployerReservationCacheCommand(
                        routeModel.EmployerAccountId, routeModel.AccountLegalEntityPublicHashedId,
                        viewModel.CohortReference);
                    
                    if (cacheReservationEmployerCommand == null)
                    {
                        _logger.LogWarning($"Account legal entity not found [{routeModel.AccountLegalEntityPublicHashedId}].");
                        return RedirectToRoute(RouteNames.Error500);
                    }
                }

                var availableReservationsResult = await _mediator.Send(
                    new GetAvailableReservationsQuery {AccountId = cacheReservationEmployerCommand.AccountId});

                if (availableReservationsResult.Reservations != null &&
                    availableReservationsResult.Reservations.Any())
                {
                    viewModel.AvailableReservations = availableReservationsResult.Reservations
                        .Select(reservation => new AvailableReservationViewModel(reservation));
                    viewModel.AccountId = cacheReservationEmployerCommand.AccountId;
                    viewModel.BackLink = backUrl;
                    return View(viewName, viewModel);
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
                _logger.LogWarning(e, $"Provider (UKPRN: {e.UkPrn}) does not has access to create a reservation for legal entity for account (Id: {e.AccountId}).");
                return View("NoPermissions", backUrl);
            }
            catch (ReservationLimitReachedException)
            {
                return View("ReservationLimitReached", backUrl);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error trying to render select reservation.");
                return RedirectToRoute(RouteNames.Error500);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [DasAuthorize(CommitmentOperation.AccessCohort)]
        [Route("{ukPrn}/reservations/{accountLegalEntityPublicHashedId}/select", Name = RouteNames.ProviderSelect)]
        [Route("accounts/{employerAccountId}/reservations/{accountLegalEntityPublicHashedId}/select", Name = RouteNames.EmployerSelect)]
        public async Task<IActionResult> PostSelectReservation(
            ReservationsRouteModel routeModel,
            SelectReservationViewModel viewModel)
        {

            var backUrl = string.Empty;

            if (viewModel.SelectedReservationId == Guid.Empty)
            {
                var availableReservationsResult = await _mediator.Send(
                    new GetAvailableReservationsQuery { AccountId = viewModel.AccountId });

                viewModel.AvailableReservations = availableReservationsResult.Reservations
                    .Select(reservation => new AvailableReservationViewModel(reservation));

                ModelState.AddModelError(nameof(viewModel.SelectedReservationId), "Select an option");

                viewModel.BackLink = _urlHelper.GenerateUrl(new UrlParameters
                {
                    Id = routeModel.UkPrn.Value.ToString(),
                    Controller = $"apprentices/{viewModel.CohortReference}",
                    Action = "details"
                });

                return View("ProviderSelect", viewModel);
            }

            if (viewModel.SelectedReservationId.HasValue &&
                viewModel.SelectedReservationId != Guid.Empty && 
                viewModel.SelectedReservationId != Guid.Parse(Guid.Empty.ToString().Replace("0", "9")))
            {
                var reservation = await _mediator.Send(new GetReservationQuery {Id = viewModel.SelectedReservationId.Value});

                var url = _urlHelper.GenerateAddApprenticeUrl(
                    viewModel.SelectedReservationId.Value,
                    routeModel.AccountLegalEntityPublicHashedId, 
                    reservation.Course.Id, 
                    routeModel.UkPrn, 
                    reservation.StartDate,
                    viewModel.CohortReference,
                    routeModel.EmployerAccountId);

                var addApprenticeUrl = url;

                return Redirect(addApprenticeUrl);
            }

            try
            {
                CacheReservationEmployerCommand cacheReservationEmployerCommand;

                if (routeModel.UkPrn.HasValue)
                {
                    backUrl = _urlHelper.GenerateUrl(new UrlParameters
                    {
                        Id = routeModel.UkPrn.Value.ToString(),
                        Controller = $"apprentices/{viewModel.CohortReference}",
                        Action = "details"
                    });

                    var response = await _mediator.Send(new GetProviderCacheReservationCommandQuery
                    {
                        AccountLegalEntityPublicHashedId = routeModel.AccountLegalEntityPublicHashedId,
                        CohortRef = routeModel.CohortReference,
                        CohortId = _encodingService.Decode(routeModel.CohortReference, EncodingType.CohortReference),
                        UkPrn = routeModel.UkPrn.Value
                    });

                    cacheReservationEmployerCommand = response.Command;
                }
                else
                {
                    cacheReservationEmployerCommand = await BuildEmployerReservationCacheCommand(
                        routeModel.EmployerAccountId, routeModel.AccountLegalEntityPublicHashedId,
                        viewModel.CohortReference);
                }
           
                await _mediator.Send(cacheReservationEmployerCommand);

                routeModel.Id = cacheReservationEmployerCommand.Id;
            }
            catch (ReservationLimitReachedException)
            {
                return View("ReservationLimitReached", backUrl);
            }
            catch (ProviderNotAuthorisedException e)
            {
                _logger.LogWarning(e, $"Provider (UKPRN: {e.UkPrn}) does not has access to create a reservation for legal entity for account (Id: {e.AccountId}).");
                return View("NoPermissions", backUrl);
            }

            return RedirectToRoute(RouteNames.ProviderApprenticeshipTraining, routeModel);
        }

        private async Task<CacheReservationEmployerCommand> BuildEmployerReservationCacheCommand(string employerAccountId, string accountLegalEntityPublicHashedId, string cohortRef)
        {
            var accountId = _encodingService.Decode(employerAccountId, EncodingType.AccountId);
            var accountLegalEntity = await _mediator.Send(new GetLegalEntitiesQuery { AccountId = accountId });
            var legalEntity = accountLegalEntity.AccountLegalEntities.SingleOrDefault(c =>
                c.AccountLegalEntityPublicHashedId.Equals(accountLegalEntityPublicHashedId));

            if (legalEntity == null)
            {
                return null;
            }

            return new CacheReservationEmployerCommand
            {
                AccountLegalEntityName = legalEntity.AccountLegalEntityName,
                AccountLegalEntityPublicHashedId = accountLegalEntityPublicHashedId,
                AccountId = accountId,
                AccountLegalEntityId = legalEntity.AccountLegalEntityId,
                Id = Guid.NewGuid(),
                CohortRef = cohortRef
            };
        }

        private async Task<ApprenticeshipTrainingViewModel> BuildApprenticeshipTrainingViewModel(
            bool isProvider,
            string accountLegalEntityPublicHashedId,
            string courseId = null, 
            TrainingDateModel selectedTrainingDate = null, 
            bool? routeModelFromReview = false,
            string cohortRef = "",
            uint? ukPrn = null)

        {
            var accountLegalEntityId = _encodingService.Decode(
                accountLegalEntityPublicHashedId,
                EncodingType.PublicAccountLegalEntityId);
            var dates = await _trainingDateService.GetTrainingDates(accountLegalEntityId);

            var coursesResult = await _mediator.Send(new GetCoursesQuery());

            return new ApprenticeshipTrainingViewModel
            {
                RouteName = isProvider ? RouteNames.ProviderCreateApprenticeshipTraining : RouteNames.EmployerCreateApprenticeshipTraining,
                PossibleStartDates = dates.Select(startDateModel => new TrainingDateViewModel(startDateModel, startDateModel.Equals(selectedTrainingDate))).OrderBy(model => model.StartDate),
                Courses = coursesResult.Courses?.Select(course => new CourseViewModel(course, courseId)),
                CourseId = courseId,
                AccountLegalEntityPublicHashedId = accountLegalEntityPublicHashedId,
                IsProvider = isProvider,
                CohortRef = cohortRef,
                FromReview = routeModelFromReview,
                BackLink = isProvider ?
                    GetProviderBackLinkForApprenticeshipTrainingView(routeModelFromReview, cohortRef, ukPrn) 
                    : routeModelFromReview.HasValue && routeModelFromReview.Value ? RouteNames.EmployerReview : RouteNames.EmployerSelectCourse 
            };
        }

        private string GetProviderBackLinkForApprenticeshipTrainingView(bool? routeModelFromReview, string cohortRef, uint? ukPrn)
        {
            if (string.IsNullOrEmpty(cohortRef))
            {
                return routeModelFromReview.HasValue && routeModelFromReview.Value ? RouteNames.ProviderReview : RouteNames.ProviderConfirmEmployer;
            }

            return _urlHelper.GenerateUrl(new UrlParameters
            {
                Id = ukPrn.ToString(),
                Controller = $"apprentices/{cohortRef}",
                Action = "details"
            }); ;
        }
    }
}
