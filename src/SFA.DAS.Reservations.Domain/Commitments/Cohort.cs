namespace SFA.DAS.Reservations.Domain.Commitments
{
    public enum CohortParty : short
    {
        None = 0,
        Employer = 1,
        Provider = 2,
        TransferSender = 4
    }

    public class Cohort
    {
        public long CohortId { get; set; }
        public string LegalEntityId { get; set; }
        public string LegalEntityName { get; set; }
        public string ProviderName { get; set; }
        public bool IsFundedByTransfer { get; set; }
        public CohortParty WithParty { get; set; }
    }
}
