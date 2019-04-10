using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Reservations.Application.Reservations.Commands.CacheReservationCourse;
using SFA.DAS.Reservations.Application.Reservations.Commands.CacheReservationEmployer;
using SFA.DAS.Reservations.Application.Reservations.Queries.GetCachedReservation;
using SFA.DAS.Reservations.Application.Reservations.Queries.GetCourses;
using SFA.DAS.Reservations.Application.Reservations.Services;
using SFA.DAS.Reservations.Domain.Courses;
using SFA.DAS.Reservations.Infrastructure.Exceptions;
using SFA.DAS.Reservations.Web.Infrastructure;
using SFA.DAS.Reservations.Web.Models;

namespace SFA.DAS.Reservations.Web.Controllers
{
    [Authorize(Policy = nameof(PolicyNames.HasEmployerAccount))]
    [Route("accounts/{employerAccountId}/reservations")]
    public class EmployerReservationsController : Controller
    {
        private readonly IMediator _mediator;
        private readonly IHashingService _hashingService;

        public EmployerReservationsController(IMediator mediator, IHashingService hashingService)
        {
            _mediator = mediator;
            _hashingService = hashingService;
        }

        // GET
        public async Task<IActionResult> Index(ReservationsRouteModel routeModel)
        {
            var accountId = _hashingService.DecodeValue(routeModel.EmployerAccountId);

            var reservationId = Guid.NewGuid();

            await _mediator.Send(new CacheReservationEmployerCommand
            {
                Id = reservationId,
                AccountId = accountId,
                AccountLegalEntityId = 1,
                AccountLegalEntityPublicHashedId = "111ABC",
                AccountLegalEntityName = "Test Corp"
            });

            var viewModel = new ReservationViewModel{ Id = reservationId};

            return View(viewModel);
        }

        [HttpGet]
        [Route("{id}/select-legal-entity", Name = RouteNames.EmployerSelectLegalEntity)]
        public async Task<IActionResult> SelectLegalEntity(ReservationsRouteModel routeModel)
        {
            var user = User;//.Claims - can get required stuff from here. wrap into different service for testing?
            await Task.CompletedTask;
            return RedirectToRoute(RouteNames.EmployerConfirmLegalEntity, routeModel);
        }

        [HttpGet]
        [Route("{id}/confirm-legal-entity", Name = RouteNames.EmployerConfirmLegalEntity)]
        public async Task<IActionResult> ConfirmLegalEntity(ReservationsRouteModel routeModel)
        {
            await Task.CompletedTask;
            return await PostConfirmLegalEntity(routeModel);
        }

        [HttpPost]
        [Route("{id}/confirm-legal-entity", Name = RouteNames.EmployerConfirmLegalEntity)]
        public async Task<IActionResult> PostConfirmLegalEntity(ReservationsRouteModel routeModel)
        {
            await Task.CompletedTask;
            return RedirectToRoute(RouteNames.EmployerSelectCourse, routeModel);
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
                Courses = courseViewModels
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
        [Route("{id}/select-course")]
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