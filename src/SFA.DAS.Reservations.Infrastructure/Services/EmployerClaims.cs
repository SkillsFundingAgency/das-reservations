namespace SFA.DAS.Reservations.Infrastructure.Services
{
    public static class EmployerClaims
    {
        public static string IdamsUserIdClaimTypeIdentifier => "http://das/employer/identity/claims/id";
        public static string AccountsClaimsTypeIdentifier => "http://das/employer/identity/claims/associatedAccounts";
    }
}