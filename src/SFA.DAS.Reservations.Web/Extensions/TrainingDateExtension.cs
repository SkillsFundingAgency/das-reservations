using System;
using SFA.DAS.Reservations.Domain.Rules;

namespace SFA.DAS.Reservations.Web.Extensions
{
    public static class TrainingDateExtension
    {
        public static string GetGDSDateString(this TrainingDateModel model)
        {
            return  model.EndDate == default(DateTime) ? 
                $"{model.StartDate.GetGDSLongDateString()}" : 
                $"{model.StartDate.GetGDSShortDateString()} to {model.EndDate.GetGDSShortDateString()}";
        }
    }
}
