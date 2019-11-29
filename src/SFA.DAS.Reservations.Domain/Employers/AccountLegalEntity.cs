using SFA.DAS.Common.Domain.Types;

namespace SFA.DAS.Reservations.Domain.Employers
{
    public class AccountLegalEntity
    {
        public long AccountId { get; set; }
        public long LegalEntityId { get; set; }
        public string AccountLegalEntityName { get; set; }
        public long AccountLegalEntityId { get; set; }
        public string AccountLegalEntityPublicHashedId { get; set; }
        public int ReservationLimit { get; set; }
        public bool IsLevy { get; set; }
        public AgreementType AgreementType { get; set; }
        public bool AgreementSigned { get; set; }
        public string AccountName { get; set; }
        public string AccountPublicHashedId { get ; set ; }
    }
}