namespace SFA.DAS.Reservations.Web.Infrastructure
{
    public static class EmployerClaims
    {
        public static string IdamsUserIdClaimTypeIdentifier => "http://das/employer/identity/claims/id";
        public static string AccountsClaimsTypeIdentifier => "http://das/employer/identity/claims/associatedAccounts";
    }
}