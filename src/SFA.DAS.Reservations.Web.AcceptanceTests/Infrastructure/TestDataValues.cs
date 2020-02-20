namespace SFA.DAS.Reservations.Web.AcceptanceTests.Infrastructure
{
    public static class TestDataValues
    {
        public const long NonLevyAccountId = 1;
        public const long NonLevyAccountLegalEntityId = 123;
        public const string NonLevyPublicHashedAccountId = "FEA456";
        public const string NonLevyHashedAccountId = "ABC123";
        public const string NonLevyHashedAccountLegalEntityId = "5UM71N6";

        public const long LevyAccountId = 2;
        public const long LevyAccountLegalEntityId = 456;
        public const string LevyPublicHashedAccountId = "AEF456";
        public const string LevyHashedAccountId = "DEF456";
        public const string LevyHashedAccountLegalEntityId = "N077H15";

        public static string CohortReference = "TGB567";
        public static string DashboardUrl = "providerdashboard.local.test.com";
        public static string EmployerDashboardUrl = "employerdashboard.local.test.com";
        public static string EmployerAccountId = "ABC123";
        public static string EmployerApprenticeUrl = "employerapprentice.local.test.com";
        public static uint ProviderId = 4568790;
    }
}