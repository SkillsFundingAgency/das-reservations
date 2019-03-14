using System.Collections.Generic;
using System.Threading.Tasks;
using SFA.DAS.Reservations.Web.Models;

namespace SFA.DAS.Reservations.Web.Services
{
    public interface IStartDateService
    {
        Task<IEnumerable<StartDateModel>> GetStartDates();
    }
}