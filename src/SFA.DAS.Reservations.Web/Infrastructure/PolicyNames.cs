namespace SFA.DAS.Reservations.Web.Infrastructure;

public static class PolicyNames
{
    public static string HasEmployerAccount => nameof(HasEmployerAccount);
    public static string HasProviderAccount => nameof(HasProviderAccount);
    public static string HasProviderOrEmployerAccount => nameof(HasProviderOrEmployerAccount);
    public static string HasEmployerViewerUserRoleOrIsProvider => nameof(HasEmployerViewerUserRoleOrIsProvider);
    public static string HasProviderGotViewerOrHigherRoleOrIsEmployer => nameof(HasProviderGotViewerOrHigherRoleOrIsEmployer);
    public static string HasProviderGotContributorOrHigherRoleOrIsEmployer => nameof(HasProviderGotContributorOrHigherRoleOrIsEmployer);
    public static string AccessCohortOrAllowEmptyCohort => nameof(AccessCohortOrAllowEmptyCohort);
}