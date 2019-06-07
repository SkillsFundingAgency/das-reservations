using System.Collections.Generic;
using System.Threading.Tasks;
using SFA.DAS.Reservations.Domain.Rules;

namespace SFA.DAS.Reservations.Web.Services
{
    public interface ITrainingDateService
    {
        Task<IEnumerable<TraningDateModel>> GetTrainingDates(long accountLegalEntityId);
    }
}