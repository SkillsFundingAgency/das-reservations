using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Reservations.Application.Employers.Queries;
using SFA.DAS.Reservations.Web.Infrastructure;
using SFA.DAS.Reservations.Web.Models;

namespace SFA.DAS.Reservations.Web.Controllers
{
    [Authorize(Policy = nameof(PolicyNames.HasProviderAccount))]
    [Route("{ukPrn}/reservations")]
    public class ProviderReservationsController : Controller
    {
        private readonly IMediator _mediator;

        public ProviderReservationsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        public IActionResult Index()
        {
            return View();
        }

        
        [Route("chooseEmployer")]
        public async Task<IActionResult> ChooseEmployer(uint ukPrn)
        {
            var employers = await _mediator.Send(new GetTrustedEmployersQuery {UkPrn = ukPrn});

            var viewModel = new ChooseEmployerViewModel
            {
                Employers = employers.Employers
            };

            return View(viewModel);
        }

        
        [Route("confirmEmployer")]
        public IActionResult ConfirmEmployer(ConfirmEmployerViewModel viewModel)
        {
            return View(viewModel);
        }
    }
}