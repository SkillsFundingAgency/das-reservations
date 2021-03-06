﻿using System;

namespace SFA.DAS.Reservations.Web.Extensions
{
    public static class DateTimeExtension
    {
        public static string GetGDSLongDateString(this DateTime datetime)
        {
            return datetime.ToString("MMMM yyyy");
        }
        public static string GetGDSShortDateString(this DateTime datetime)
        {
            return datetime.ToString("MMM yyyy");
        }
    }
}
