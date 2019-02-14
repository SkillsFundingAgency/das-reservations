using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.Reservations.Web.Services
{
    public interface IStartDateService
    {
        Task<IEnumerable<DateTime>> GetStartDates();
    }
}