using System.Collections.Generic;
using System.Linq;
using SFA.DAS.Reservations.Domain.Employers;

namespace SFA.DAS.Reservations.Web.Models
{
    public class SelectLegalEntityViewModel
    {
        public SelectLegalEntityViewModel(ReservationsRouteModel routeModel,
            IEnumerable<AccountLegalEntity> accountLegalEntities, long? selectedLegalEntity)
        {
            RouteModel = routeModel;
            LegalEntities = accountLegalEntities?.Select(apiModel => new LegalEntityViewModel(apiModel, selectedLegalEntity));
        }

        public IEnumerable<LegalEntityViewModel> LegalEntities { get; }
        public ReservationsRouteModel RouteModel { get; }
    }

    public class LegalEntityViewModel
    {
        public LegalEntityViewModel(AccountLegalEntity accountLegalEntity, long?selectedLegalEntity)
        {
            Name = accountLegalEntity.Name;
            AccountLegalEntityPublicHashedId = accountLegalEntity.AccountLegalEntityPublicHashedId;
            Selected = selectedLegalEntity.HasValue && selectedLegalEntity.Value == accountLegalEntity.AccountLegalEntityId;
        }

        public bool Selected { get; set; }

        public string Name { get; }
        public string AccountLegalEntityPublicHashedId { get; }
    }
}