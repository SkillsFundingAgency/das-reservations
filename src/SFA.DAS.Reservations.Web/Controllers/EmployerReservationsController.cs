using System;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Reservations.Application.Reservations.Commands;
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

        [Route("{reservationId}/SelectCourse")]
        public async Task<IActionResult> SelectCourse(Guid reservationId)
        {
            var getCoursesResponse = await _mediator.Send(new GetCoursesQuery());

            var courseViewModels = getCoursesResponse.Courses.Select(c => new CourseViewModel(c));

            var viewModel = new EmployerSelectCourseViewModel
            {
                ReservationId = reservationId,
                Courses = courseViewModels
            };

            return View(viewModel);
        }
    }
}