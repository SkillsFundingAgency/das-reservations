using System;
using SFA.DAS.Reservations.Domain.Rules;

namespace SFA.DAS.Reservations.Web.Extensions
{
    public static class TrainingDateExtension
    {
        public static string GetGDSShortDateString(this TrainingDateModel model)
        {
            return  model.EndDate == default(DateTime) ? 
                $"{model.StartDate:MMMM yyyy}" : 
                $"{model.StartDate:MMM yyyy} to {model.EndDate:MMM yyyy}";
        }
    }
}
