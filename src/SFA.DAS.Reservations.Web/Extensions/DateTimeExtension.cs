using System;

namespace SFA.DAS.Reservations.Web.Extensions
{
    public static class DateTimeExtension
    {
        public static string GetGDSShortDateString(this DateTime datetime)
        {
            return datetime.ToString("MMMM yyyy");
        }
    }
}
