namespace SFA.DAS.Reservations.Web.Infrastructure
{
    public static class PolicyNames
    {
        public static string HasEmployerAccount => nameof(HasEmployerAccount);
        public static string HasProviderAccount => nameof(HasProviderAccount);
        public static string HasProviderOrEmployerAccount => nameof(HasProviderOrEmployerAccount);
        public static string HasEmployerViewerUserRole => nameof(HasEmployerViewerUserRole);
    }
}