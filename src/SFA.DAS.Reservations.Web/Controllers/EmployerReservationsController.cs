using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Reservations.Application.Reservations.Commands;
using SFA.DAS.Reservations.Application.Reservations.Queries;
using SFA.DAS.Reservations.Application.Reservations.Queries.GetCourses;
using SFA.DAS.Reservations.Application.Reservations.Services;
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
        public async Task<IActionResult> Index(string employerAccountId)
        {
            var accountId = _hashingService.DecodeValue(employerAccountId);

            var response = await _mediator.Send(new CacheCreateReservationCommand
            {
                AccountId = accountId,
                AccountLegalEntityId = 1,
                AccountLegalEntityName = "Test Corp"
            });

            var viewModel = new ReservationViewModel{ Id = response.Id};

            return View(viewModel);
        }

        [HttpGet]
        [Route("{id}/SelectCourse",Name = RouteNames.EmployerSelectCourse)]
        public async Task<IActionResult> SelectCourse(Guid id)
        {
            var getCoursesResponse = await _mediator.Send(new GetCoursesQuery());

            var courseViewModels = getCoursesResponse.Courses.Select(c => new CourseViewModel(c));

            var viewModel = new EmployerSelectCourseViewModel
            {
                ReservationId = id,
                Courses = courseViewModels
            };

            return View(viewModel);
        }

        [HttpPost]
        [Route("{id}/SelectCourse")]
        public async Task<IActionResult> PostSelectCourse(ReservationsRouteModel routeModel, string selectedCourseId)
        {
            var cachedReservation = await _mediator.Send(new GetCachedReservationQuery {Id = routeModel.Id.Value});

            if (cachedReservation == null)
            {
                throw new ArgumentException("Reservation not found", nameof(routeModel.Id));
            }

            if (string.IsNullOrEmpty(selectedCourseId))
            {
                return RedirectToRoute(RouteNames.EmployerApprenticeshipTraining, new
                {
                    Id = cachedReservation.Id,
                    EmployerAccountId = routeModel.EmployerAccountId,
                });
            }
         
            var getCoursesResult = await _mediator.Send(new GetCoursesQuery());
            var selectedCourse = getCoursesResult.Courses.SingleOrDefault(c => c.Id.Equals(selectedCourseId));
            var course = selectedCourse ?? throw new ArgumentException("Selected course does not exist", nameof(selectedCourseId));

            try
            {
                var result = await _mediator.Send(new CacheCreateReservationCommand
                {
                    Id = cachedReservation.Id,
                    AccountId = cachedReservation.AccountId,
                    AccountLegalEntityId = cachedReservation.AccountLegalEntityId,
                    AccountLegalEntityName = cachedReservation.AccountLegalEntityName,
                    CourseId = course.Id,
                    CourseDescription = course.CourseDescription,
                    IgnoreStartDate = true
                });

                return RedirectToRoute(RouteNames.EmployerApprenticeshipTraining, new
                {
                    Id = result.Id,
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

                var courseViewModels = getCoursesResponse.Courses.Select(c => new CourseViewModel(c));

                var viewModel = new EmployerSelectCourseViewModel
                {
                    ReservationId = routeModel.Id.Value,
                    Courses = courseViewModels
                };

                return View("SelectCourse", viewModel);
            }
        }
    }
}