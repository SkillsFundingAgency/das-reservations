using System.Collections.Generic;
using System.Linq;
using SFA.DAS.Reservations.Domain.Employers;

namespace SFA.DAS.Reservations.Web.Models
{
    public class SelectLegalEntityViewModel
    {
        public SelectLegalEntityViewModel(
            ReservationsRouteModel routeModel,
            IEnumerable<AccountLegalEntity> accountLegalEntities, 
            string selectedAccountLegalEntityPublicHashedId)
        {
            RouteModel = routeModel;
            LegalEntities = accountLegalEntities?.Select(apiModel => new LegalEntityViewModel(apiModel, selectedAccountLegalEntityPublicHashedId));
        }

        public IEnumerable<LegalEntityViewModel> LegalEntities { get; }
        public ReservationsRouteModel RouteModel { get; }
    }

    public class LegalEntityViewModel
    {
        public LegalEntityViewModel(AccountLegalEntity accountLegalEntity, string selectedAccountLegalEntityPublicHashedId)
        {
            Name = accountLegalEntity.AccountLegalEntityName;
            AccountLegalEntityPublicHashedId = accountLegalEntity.AccountLegalEntityPublicHashedId;
            Selected = !string.IsNullOrWhiteSpace(selectedAccountLegalEntityPublicHashedId) && selectedAccountLegalEntityPublicHashedId == accountLegalEntity.AccountLegalEntityPublicHashedId;
        }

        public bool Selected { get; set; }

        public string Name { get; }
        public string AccountLegalEntityPublicHashedId { get; }
    }
}