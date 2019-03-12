namespace SFA.DAS.Reservations.Domain.Employers
{
    public class Employer
    {
        public long AccountId { get; set; }
        public string AccountPublicHashedId { get; set; }
        public string AccountName { get; set; }
        public long LegalEntityId { get; set; }
        public string LegalEntityPublicHashedId { get; set; }
        public string LegalEntityName { get; set; }
    }
}
