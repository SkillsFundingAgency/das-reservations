using System;

namespace SFA.DAS.Reservations.Web.Extensions
{
    public static class DateTimeExtension
    {
        public static string GetDasShortDateString(this DateTime datetime)
        {
            return datetime.ToString("MMM yyyy");
        }
    }
}
