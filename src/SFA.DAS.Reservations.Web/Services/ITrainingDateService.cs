using System.Collections.Generic;
using System.Threading.Tasks;
using SFA.DAS.Reservations.Application.FundingRules.Queries.GetAvailableDates;
using SFA.DAS.Reservations.Domain.Rules;

namespace SFA.DAS.Reservations.Web.Services
{
    public interface ITrainingDateService
    {
        Task<GetAvailableDatesResult> GetTrainingDates(long accountLegalEntityId);
    }
}