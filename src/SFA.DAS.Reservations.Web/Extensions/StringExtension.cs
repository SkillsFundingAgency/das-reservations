namespace SFA.DAS.Reservations.Web.Extensions
{
    public static class StringExtension
    {
        public static bool? ToBoolean(this string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            if (!bool.TryParse(value, out var result))
            {
                return null;
            }

            return result;
        }
    }
}
