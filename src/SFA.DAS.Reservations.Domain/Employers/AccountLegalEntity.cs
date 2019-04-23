namespace SFA.DAS.Reservations.Domain.Employers
{
    public class AccountLegalEntity
    {
        public string DasAccountId { get; set; }
        public long LegalEntityId { get; set; }
        public string Name { get; set; }
        public long AccountLegalEntityId { get; set; }
        public string AccountLegalEntityPublicHashedId { get; set; }
    }
}