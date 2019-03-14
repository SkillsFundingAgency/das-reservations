using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Web.Models;

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
                var model = BuildStartDateModel(now.AddMonths(i));
                datesToReturn.Add(model);
            }
            return await Task.FromResult(datesToReturn);
        }

        private StartDateModel BuildStartDateModel(DateTime now)
        {
            var startDate = now.AddDays(1 - now.Day).Date;
            var threeMonthsFromNow = now.AddMonths(3);
            var lastDayOfTheMonth = DateTime.DaysInMonth(threeMonthsFromNow.Year, threeMonthsFromNow.Month);
            var expiryDate = new DateTime(threeMonthsFromNow.Year, threeMonthsFromNow.Month, lastDayOfTheMonth);

            return new StartDateModel
            {
                StartDate = startDate,
                ExpiryDate = expiryDate
            };
        }
    }
}