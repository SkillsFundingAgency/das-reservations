using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SFA.DAS.Reservations.Domain.Interfaces;

namespace SFA.DAS.Reservations.Web.Services
{
    public class StartDateService : IStartDateService
    {
        private readonly ICurrentDateTime _currentDateTime;

        public StartDateService(ICurrentDateTime currentDateTime)
        {
            _currentDateTime = currentDateTime;
        }

        public async Task<IEnumerable<StartDateModel>> GetStartDates()
        {
            var now = _currentDateTime.Now;
            var datesToReturn = new List<StartDateModel>();
            for (var i = 0; i < 6; i++)
            {
                var startDate = now.AddMonths(i).AddDays(1-now.Day).Date;
                datesToReturn.Add(new StartDateModel
                {
                    StartDate = startDate
                });
            }
            return await Task.FromResult(datesToReturn);
        }
    }
}