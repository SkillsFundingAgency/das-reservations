using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Reservations.Application.FundingRules.Queries.GetNextUnreadGlobalFundingRule;
using SFA.DAS.Reservations.Domain.Rules.Api;
using SFA.DAS.Reservations.Web.Infrastructure;
using SFA.DAS.Reservations.Web.Models;

namespace SFA.DAS.Reservations.Web.Controllers
{
    public abstract class ReservationsBaseController : Controller
    {
        private readonly IMediator _mediator;

        protected ReservationsBaseController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [Route("CheckNextGlobalRule")]
        public async Task<ViewResult> CheckNextGlobalRule(string redirectRouteName, string claimName, string backLink)
        {
            var userAccountIdClaim = User.Claims.First(c => c.Type.Equals(claimName));
            var response = await _mediator.Send(new GetNextUnreadGlobalFundingRuleQuery { Id = userAccountIdClaim.Value });

            var nextGlobalRuleId = response?.Rule?.Id;
            var nextGlobalRuleStartDate = response?.Rule?.ActiveFrom;

            if (!nextGlobalRuleId.HasValue || nextGlobalRuleId.Value == 0 || !nextGlobalRuleStartDate.HasValue)
            {
                return null;
            }

            var viewModel = new FundingRestrictionNotificationViewModel
            {
                RuleId = nextGlobalRuleId.Value,
                TypeOfRule = RuleType.GlobalRule,
                RestrictionStartDate = nextGlobalRuleStartDate.Value,
                BackLink = backLink,
                RouteName = redirectRouteName
            };

            return View("FundingRestrictionNotification", viewModel);
        }

    }
}