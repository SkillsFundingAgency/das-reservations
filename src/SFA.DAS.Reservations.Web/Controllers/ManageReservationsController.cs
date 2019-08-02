using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Common.Domain.Types;
using SFA.DAS.Encoding;
using SFA.DAS.Reservations.Application.Employers.Queries;
using SFA.DAS.Reservations.Application.Employers.Queries.GetLegalEntities;
using SFA.DAS.Reservations.Application.Reservations.Queries.GetReservations;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Web.Infrastructure;
using SFA.DAS.Reservations.Web.Models;

namespace SFA.DAS.Reservations.Web.Controllers
{
    [Authorize(Policy = nameof(PolicyNames.HasProviderOrEmployerAccount))]
    public class ManageReservationsController : Controller
    {
        private readonly IMediator _mediator;
        private readonly IEncodingService _encodingService;
        private readonly IExternalUrlHelper _urlHelper;

        public ManageReservationsController(
            IMediator mediator, 
            IEncodingService encodingService,
            IExternalUrlHelper urlHelper)
        {
            _mediator = mediator;
            _encodingService = encodingService;
            _urlHelper = urlHelper;
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
                var result = await _mediator.Send(new GetLegalEntitiesQuery
                    {
                        AccountId = decodedAccountId
                    });
                if (result.AccountLegalEntities.Any(entity =>
                    !entity.IsLevy && 
                    entity.AgreementType != AgreementType.NonLevyExpressionOfInterest))
                {
                    var homeLink = _urlHelper.GenerateUrl(new UrlParameters
                    {
                        Controller = "teams",
                        SubDomain = "accounts",
                        Folder = "accounts",
                        Id = routeModel.EmployerAccountId
                    });

                    return View("NonEoiHolding", new NonEoiHoldingViewModel
                    {
                        BackLink = homeLink,
                        HomeLink = homeLink
                    });
                }

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
    }
}