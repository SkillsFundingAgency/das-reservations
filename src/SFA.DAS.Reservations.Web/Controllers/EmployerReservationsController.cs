using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Reservations.Application.Reservations.Queries.GetCourses;
using SFA.DAS.Reservations.Web.Infrastructure;
using SFA.DAS.Reservations.Web.Models;

namespace SFA.DAS.Reservations.Web.Controllers
{
    [Authorize(Policy = nameof(PolicyNames.HasEmployerAccount))]
    [Route("accounts/{employerAccountId}/reservations")]
    public class EmployerReservationsController : Controller
    {
        private readonly IMediator _mediator;

        public EmployerReservationsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // GET
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> ViewCourses()
        {
            var getCoursesResponse = await _mediator.Send(new GetCoursesQuery());

            var courseViewModels = getCoursesResponse.Courses.Select(c => new CourseViewModel(c));

            var viewModel = new EmployerCoursesViewModel
            {
                Courses = courseViewModels
            };

            return View(viewModel);
        }
    }
}