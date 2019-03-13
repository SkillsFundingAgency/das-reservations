namespace SFA.DAS.Reservations.Domain.Employers
{
    public class Employer
    {
        public long AccountId { get; set; }
        public string AccountPublicHashedId { get; set; }
        public string AccountName { get; set; }
        public long AccountLegalEntityId { get; set; }
        public string AccountLegalEntityPublicHashedId { get; set; }
        public string AccountLegalEntityName { get; set; }
    }
}
