using System.Collections.Generic;
using System.Linq;
using SFA.DAS.Reservations.Domain.Employers;

namespace SFA.DAS.Reservations.Web.Models
{
    public class SelectLegalEntityViewModel
    {
        public SelectLegalEntityViewModel(
            ReservationsRouteModel routeModel,
            IEnumerable<AccountLegalEntity> accountLegalEntities)
        {
            RouteModel = routeModel;
            LegalEntities = accountLegalEntities?.Select(apiModel => new LegalEntityViewModel(apiModel));
        }

        public IEnumerable<LegalEntityViewModel> LegalEntities { get; }
        public ReservationsRouteModel RouteModel { get; }
    }

    public class LegalEntityViewModel
    {
        public LegalEntityViewModel(AccountLegalEntity accountLegalEntity)
        {
            Name = accountLegalEntity.Name;
            AccountLegalEntityPublicHashedId = accountLegalEntity.AccountLegalEntityPublicHashedId;
        }

        public string Name { get; }
        public string AccountLegalEntityPublicHashedId { get; }
    }
}