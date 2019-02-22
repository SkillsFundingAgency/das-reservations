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

        public async Task<IEnumerable<DateTime>> GetStartDates()
        {
            var now = _currentDateTime.Now;
            var datesToReturn = new List<DateTime>();
            for (var i = 0; i < 6; i++)
            {
                var dateToAdd = now.AddMonths(i).AddDays(1-now.Day).Date;
                datesToReturn.Add(dateToAdd);
            }
            return await Task.FromResult(datesToReturn);
        }
    }
}