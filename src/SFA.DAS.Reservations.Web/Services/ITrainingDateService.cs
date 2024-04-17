using System.Threading.Tasks;
using SFA.DAS.Reservations.Application.FundingRules.Queries.GetAvailableDates;

namespace SFA.DAS.Reservations.Web.Services
{
    public interface ITrainingDateService
    {
        Task<GetAvailableDatesResult> GetTrainingDates(long accountLegalEntityId);
    }
}