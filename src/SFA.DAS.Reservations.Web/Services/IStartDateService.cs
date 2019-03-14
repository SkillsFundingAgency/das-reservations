using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.Reservations.Web.Services
{
    public interface IStartDateService
    {
        Task<IEnumerable<StartDateModel>> GetStartDates();
    }

    public class StartDateModel
    {
        public DateTime StartDate { get; set; }
        public DateTime ExpiryDate { get; set; }
    }
}