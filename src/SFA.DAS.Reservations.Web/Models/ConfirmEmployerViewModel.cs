using System;

namespace SFA.DAS.Reservations.Web.Models
{
    public class ConfirmEmployerViewModel
    {   
        public uint UkPrn { get; set; }
        public string AccountPublicHashedId { get; set; }
        public string AccountName { get; set; }
        public string AccountLegalEntityPublicHashedId { get; set; }
        public string AccountLegalEntityName { get; set; }

        public bool? Confirm { get; set; }
        public Guid? Id { get; set; }
    }
}
